namespace BadVM.Shared.Memory
{

    public static class MemoryMapExtensions
    {

        private class ArrayMemoryEntry : MemoryMapEntry
        {

            private readonly byte[] m_Data;
            private readonly bool m_ReadOnly;

            #region Public

            public ArrayMemoryEntry( long address, byte[] data, bool readOnly ) : base( address, data.Length )
            {
                m_Data = data;
                m_ReadOnly = readOnly;
            }

            public override byte Read( long address )
            {
                return m_Data[address - Address];
            }

            public override void Write( long address, byte value )
            {
                if ( m_ReadOnly )
                {
                    throw new InvalidOperationException( "Attempt to write to read-only memory." );
                }

                m_Data[address - Address] = value;
            }

            #endregion

        }

        #region Public

        public static MemoryMapEntry ToMemoryEntry( this byte[] data, long address, bool readOnly = false )
        {
            return new ArrayMemoryEntry( address, data, readOnly );
        }

        #endregion

    }

}
