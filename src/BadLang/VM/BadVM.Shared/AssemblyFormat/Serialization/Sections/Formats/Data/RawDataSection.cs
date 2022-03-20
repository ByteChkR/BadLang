using BadVM.Shared.AssemblyFormat.Exceptions;
using BadVM.Shared.Memory;

namespace BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Data
{

    public class RawDataSection : AssemblySection
    {

        private readonly byte[] m_Data;

        #region Public

        public RawDataSection( Func < Assembly > asmLookup, string name, SectionMetaData metaData, byte[] data ) : base(
             asmLookup,
             name,
             SectionType.Data,
             metaData
            )
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

            MemoryMapEntry e = data.ToMemoryEntry( address, false );
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
