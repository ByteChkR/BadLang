namespace BadVM.Shared.Memory
{

    public abstract class MemoryMapEntry
    {

        public long Address { get; }

        public long Length { get; }

        #region Public

        public MemoryMapEntry( ulong address, long length ) : this( ( long )address, length )
        {
        }

        public MemoryMapEntry( long address, long length )
        {
            Address = address;
            Length = length;
        }

        public abstract byte Read( long address );

        public abstract void Write( long address, byte value );

        public override string ToString()
        {
            return $"{GetType().Name} at 0x{Address:X16} - 0x{Address + Length:X16}";
        }

        #endregion

    }

}
