using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization;
using BadVM.Shared.AssemblyFormat.Serialization.Sections;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats;
using BadVM.Shared.Logging;
using BadVM.Shared.Memory;

namespace BadVM.AssemblyFormat
{

    public class BadAssemblyDomain : IAssemblyDomain
    {

        public delegate void AssemblyResolver( BadAssemblyResolveEventArgs args );

        public static readonly LogMask LogMask = new LogMask( "AssemblyDomain" );

        private class MappedAssembly
        {

            private readonly Dictionary < AssemblySection, long > m_SectionOffsets;

            public IEnumerable < string > Sections => m_SectionOffsets.Keys.Select( x => x.Name );

            #region Public

            public MappedAssembly( Dictionary < AssemblySection, long > sectionOffsets )
            {
                m_SectionOffsets = sectionOffsets;
            }

            public long GetAddress( string section )
            {
                foreach ( KeyValuePair < AssemblySection, long > sectionOffset in m_SectionOffsets )
                {
                    if ( sectionOffset.Key.Name == section )
                    {
                        return sectionOffset.Value;
                    }
                }

                return -1;
            }

            #endregion

        }

        private static int m_NextAddress = 0x10000000;

        private readonly Dictionary < MemoryBus, Dictionary < Assembly, MappedAssembly > > m_MappedSections =
            new Dictionary < MemoryBus, Dictionary < Assembly, MappedAssembly > >();

        private readonly List < Assembly > m_LoadedAssemblies = new List < Assembly >();

        private readonly List < AssemblyResolver > m_Resolvers = new List < AssemblyResolver >();

        public IEnumerable < Assembly > LoadedAssemblies => m_LoadedAssemblies;

        public event Action < AssemblySection, long >? OnSectionMapped;

        public event Action < Assembly, string? >? OnAssemblyLoaded;

        #region Public

        public void AddResolver( AssemblyResolver resolver )
        {
            LogMask.LogMessage( $"Adding resolver {resolver}" );
            m_Resolvers.Add( resolver );
        }

        public long GetSymbolAddress( MemoryBus bus, string sectionName )
        {
            return GetSymbolAddress( bus, AssemblySymbol.Parse( sectionName ) );
        }

        public long GetSymbolAddress( MemoryBus bus, AssemblySymbol sectionName )
        {
            if ( m_MappedSections.ContainsKey( bus ) )
            {
                foreach ( KeyValuePair < Assembly, MappedAssembly > mappedSection in m_MappedSections[bus] )
                {
                    if ( mappedSection.Key.Name == sectionName.AssemblyName )
                    {
                        IEnumerable < AssemblySection > sections =
                            mappedSection.Key.GetSections( sectionName.SectionName );

                        foreach ( AssemblySection section in sections )
                        {
                            if ( section.HasSymbol( sectionName.SymbolName ) )
                            {
                                return mappedSection.Value.GetAddress( section.Name ) +
                                       section.GetSymbol( sectionName.SymbolName ).Offset;
                            }
                        }
                    }
                }
            }

            return -1;
        }

        public void Link( MemoryBus bus, Assembly asm, AssemblySection section, byte[] data )
        {
            BadAssemblyLinker.Link( this, bus, asm, section, data );
        }

        public Assembly LoadAssembly( byte[] data, string? pathHint )
        {
            AssemblyReader reader = new AssemblyReader( AssemblySectionFormat.Formats, data );

            Assembly assembly = reader.ReadAssembly( 0 );

            OnAssemblyLoaded?.Invoke( assembly, pathHint );

            if ( m_LoadedAssemblies.Any( a => a.Name == assembly.Name ) )
            {
                throw new AssemblyLoadException( "Assembly " + assembly.Name + " is already loaded" );
            }

            m_LoadedAssemblies.Add( assembly );

            foreach ( string dependency in assembly.GetDependencies() )
            {
                ResolveAssembly( dependency );
            }

            return assembly;
        }

        public void MapAssembly( string name, MemoryBus bus )
        {
            List < Action > finishActions = new List < Action >();

            Action < Action > addFinishAction = ( Action action ) => { finishActions.Add( action ); };

            LogMask.LogMessage( $"Mapping Assembly {name} to address 0x{m_NextAddress:X16}" );
            m_NextAddress = ( int )InnerMapAssembly( name, m_NextAddress, bus, addFinishAction );

            foreach ( Action finishAction in finishActions )
            {
                finishAction();
            }
        }

        public IEnumerable < Assembly > MappedAssemblies( MemoryBus bus )
        {
            if ( m_MappedSections.ContainsKey( bus ) )
            {
                foreach ( KeyValuePair < Assembly, MappedAssembly > assembly in m_MappedSections[bus] )
                {
                    yield return assembly.Key;
                }
            }
        }

        public IEnumerable < string > MappedSections( MemoryBus bus, Assembly assembly )
        {
            if ( m_MappedSections.ContainsKey( bus ) )
            {
                if ( m_MappedSections[bus].ContainsKey( assembly ) )
                {
                    return m_MappedSections[bus][assembly].Sections;
                }
            }

            throw new AssemblyLoadException( "No such assembly mapped to this bus" );
        }

        public void RemoveResolver( AssemblyResolver resolver )
        {
            LogMask.LogMessage( $"Removing resolver {resolver}" );
            m_Resolvers.Remove( resolver );
        }

        #endregion

        #region Private

        private long InnerMapAssembly( string name, long address, MemoryBus bus, Action < Action > addFinishAction )
        {
            if ( m_MappedSections.ContainsKey( bus ) && m_MappedSections[bus].Any( x => x.Key.Name == name ) )
            {
                return address;
            }

            Assembly asm = m_LoadedAssemblies.Find( a => a.Name == name );

            if ( asm == null )
            {
                throw new AssemblyLoadException( "Assembly not found" );
            }

            long current = address;
            Dictionary < AssemblySection, long > mappedSections = new Dictionary < AssemblySection, long >();

            foreach ( AssemblySection section in asm.Sections )
            {
                section.Load( current, this, bus, addFinishAction );
                mappedSections.Add( section, current );

                OnSectionMapped?.Invoke( section, current );

                current += section.GetLoadedSize();
            }

            if ( !m_MappedSections.ContainsKey( bus ) )
            {
                m_MappedSections[bus] = new Dictionary < Assembly, MappedAssembly >();
            }

            m_MappedSections[bus][asm] = new MappedAssembly( mappedSections );

            foreach ( string dependency in asm.GetDependencies() )
            {
                current = InnerMapAssembly( dependency, current, bus, addFinishAction );
            }

            return current;
        }

        private Assembly ResolveAssembly( string name )
        {
            Assembly asm = m_LoadedAssemblies.FirstOrDefault( a => a.Name == name );

            if ( asm != null )
            {
                return asm;
            }

            LogMask.LogMessage( $"Resolving assembly {name}" );
            BadAssemblyResolveEventArgs args = new BadAssemblyResolveEventArgs( name );

            foreach ( AssemblyResolver resolver in m_Resolvers )
            {
                resolver( args );

                if ( args.IsResolved && args.AssemblyBytes != null )
                {
                    return LoadAssembly( args.AssemblyBytes, args.PathHint );
                }
            }

            throw new AssemblyLoadException( "Could not resolve assembly " + name );
        }

        #endregion

    }

}
