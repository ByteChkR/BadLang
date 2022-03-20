using BadVM.Shared.AssemblyFormat.Exceptions;
using BadVM.Shared.Memory;

namespace BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Code
{

    public class RawCodeSection : AssemblySection
    {

        private readonly byte[] m_Data;

        #region Public

        public RawCodeSection( Func < Assembly > asmResolve, string name, SectionMetaData metaData, byte[] data ) :
            base( asmResolve, name, SectionType.Code, metaData )
        {
            m_Data = data;
        }

        public override int GetLoadedSize()
        {
            return m_Data.Length;
        }

        public override void Load(
            long address,
            IAssemblyDomain domain,
            MemoryBus bus,
            Action < Action > addFinishAction )
        {
            byte[] data = new byte[m_Data.Length];
            Array.Copy( m_Data, data, m_Data.Length );

            MemoryMapEntry e = data.ToMemoryEntry( address, true );
            bus.Register( e );

            addFinishAction(
                            () =>
                            {
                                if ( Assembly != null )
                                {
                                    domain.Link( bus, Assembly, this, data );
                                }
                                else
                                {
                                    throw new SectionLinkException( "No assembly to link to" );
                                }
                            }
                           );
        }

        #endregion

    }

}
