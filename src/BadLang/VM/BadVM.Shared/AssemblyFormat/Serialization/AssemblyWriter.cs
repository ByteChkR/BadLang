using System.Text;

using BadVM.Shared.AssemblyFormat.Exceptions;
using BadVM.Shared.AssemblyFormat.Serialization.Sections;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;
using BadVM.Shared.Logging;

namespace BadVM.Shared.AssemblyFormat.Serialization
{

    public class AssemblyWriter
    {

        public readonly string Name;

        private static readonly LogMask LogMask = new LogMask( "AssemblyWriter" );
        private const long MAGIC = 0x6969_6969_6969_6969;
        private readonly List < ISectionWriter > m_Data = new List < ISectionWriter >();
        private readonly List < string > m_Dependencies = new List < string >();
        private readonly IEnumerable < AssemblySectionFormat > m_Formats;

        public AssemblyCompilationDataContainer CompilationData { get; } = new AssemblyCompilationDataContainer();

        #region Public

        public AssemblyWriter( string name, IEnumerable < AssemblySectionFormat > formats )
        {
            Name = name;
            m_Formats = formats;
        }

        public void AddDependency( string name )
        {
            if ( !m_Dependencies.Contains( name ) )
            {
                LogMask.LogMessage( $"Adding dependency {name}" );
                m_Dependencies.Add( name );
            }
            else
            {
                LogMask.Warning( $"Dependency {name} already added" );
            }
        }

        public void Clear()
        {
            LogMask.LogMessage( $"Clearing Data" );
            m_Data.Clear();
        }

        public byte[] GetAssembly()
        {
            List < byte > data = new List < byte >();
            data.AddRange( BitConverter.GetBytes( MAGIC ) );
            byte[] namebuf = Encoding.UTF8.GetBytes( Name );
            data.AddRange( BitConverter.GetBytes( namebuf.Length ) );
            data.AddRange( namebuf );
            data.AddRange( BitConverter.GetBytes( m_Data.Count( x => !x.IsEmpty ) ) );

            foreach ( ISectionWriter section in m_Data )
            {
                if ( section.IsEmpty )
                {
                    continue;
                }

                AssemblySectionFormat format = m_Formats.FirstOrDefault( x => x.Name == section.SectionFormat ) ??
                                               throw new SectionFormatNotFoundException(
                                                    "Unknown section format: " + section.SectionFormat
                                                   );

                LogMask.LogMessage( $"Writing Section {section.SectionName} with format {format.Name}" );

                data.AddRange( format.Write( section ) );
            }

            data.AddRange( BitConverter.GetBytes( m_Dependencies.Count ) );

            foreach ( string dependency in m_Dependencies )
            {
                byte[] depbuf = Encoding.UTF8.GetBytes( dependency );
                data.AddRange( BitConverter.GetBytes( depbuf.Length ) );
                data.AddRange( depbuf );
            }

            return data.ToArray();
        }

        public CodeSectionWriter GetCodeWriter( string name, string format )
        {
            CodeSectionWriter w = new CodeSectionWriter( Name, name, format, this );
            LogMask.LogMessage( $"Adding Code Section {name} with format {format}" );
            m_Data.Add( w );

            return w;
        }

        public DataSectionWriter GetDataWriter( string name, string format, bool isReadOnly )
        {
            if (m_Data.Any(x => x.SectionName == name))
            {
                return (m_Data.First(x => x.SectionName == name) as DataSectionWriter)!;
            }
            DataSectionWriter w = new DataSectionWriter( format, Name, name, isReadOnly );
            LogMask.LogMessage( $"Adding Data Section {name} with format {format}" );
            m_Data.Add( w );

            return w;
        }

        public AssemblyElement GetElement( AssemblySymbol symbol )
        {
            if ( symbol.AssemblyName != Name )
            {
                throw new SymbolNotFoundException( $"Symbol '{symbol}' is not from this assembly" );
            }

            ISectionWriter writer = m_Data.FirstOrDefault( x => x.SectionName == symbol.SectionName );

            if ( writer == null )
            {
                throw new SectionNotFoundException( $"Section {symbol.SectionName} not found" );
            }

            SectionMetaData md = writer.GetMetaData();
            AssemblyElement elem = md.Elements.FirstOrDefault( x => x.Name.SymbolName == symbol.SymbolName );

            if ( elem == null )
            {
                throw new SymbolNotFoundException( $"Symbol {symbol.SymbolName} not found" );
            }

            return elem;
        }

        #endregion

    }

}
