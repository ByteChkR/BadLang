using BadVM.Shared.Logging;
using BadVM.Shared.Memory.Exceptions;

namespace BadVM.Shared.Memory
{

    public class MemoryBus
    {

        private static readonly LogMask LogMask = new LogMask( nameof( MemoryBus ) );

        private readonly List < MemoryMapEntry > m_Entries = new List < MemoryMapEntry >();

        #region Public

        public byte Read( long address )
        {
            MemoryMapEntry e = ResolveEntry( address );

            if ( e == null )
            {
                throw new ArgumentOutOfRangeException( nameof( address ) );
            }

            return e.Read( address );
        }

        public void Register( MemoryMapEntry entry )
        {
            LogMask.Info( $"Registering memory map entry: {entry}" );
            ThrowIfIntersecting( entry );
            m_Entries.Add( entry );
        }

        public void Unregister( MemoryMapEntry entry )
        {
            LogMask.Info( $"Unregistering memory map entry: {entry}" );
            m_Entries.Remove( entry );
        }

        public void Write( long address, byte value )
        {
            MemoryMapEntry e = ResolveEntry( address );

            e.Write( address, value );
        }

        #endregion

        #region Private

        private MemoryMapEntry ResolveEntry( long address )
        {
            MemoryMapEntry? e = m_Entries.FirstOrDefault( x => x.Address <= address && x.Address + x.Length > address );

            if ( e == null )
            {
                throw new MemoryUnmappedException(
                                                  $"Address 0x{address:X16} is not mapped on the Memory Bus",
                                                  address
                                                 );
            }

            return e;
        }

        private void ThrowIfIntersecting( MemoryMapEntry entry )
        {
            foreach ( MemoryMapEntry e in m_Entries )
            {
                if ( e.Address + e.Length <= entry.Address || e.Address >= entry.Address + entry.Length )
                {
                    continue;
                }

                throw new MemoryMapConflictException( e, entry );
            }
        }

        #endregion

    }

}
