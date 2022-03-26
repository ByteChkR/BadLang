namespace BadVM.Shared.Memory
{

    public static class MemoryBusExtensions
    {

        #region Public

        public static void Copy( this MemoryBus bus, long from, long to, int size )
        {
            byte[] buf = bus.Read( from, size );
            bus.Write( to, buf );
        }

        public static byte[] Read( this MemoryBus bus, long address, int length )
        {
            byte[] data = new byte[length];
            bus.Read( data, 0, address, length );

            return data;
        }

        public static void Read( this MemoryBus bus, byte[] buffer, int start, long address, int length )
        {
            for ( int i = 0; i < length; i++ )
            {
                buffer[start + i] = bus.Read( address + i );
            }
        }

        public static short ReadInt16( this MemoryBus bus, long address )
        {
            return BitConverter.ToInt16( bus.Read( address, sizeof( short ) ), 0 );
        }

        public static int ReadInt32( this MemoryBus bus, long address )
        {
            return BitConverter.ToInt32( bus.Read( address, sizeof( int ) ), 0 );
        }

        public static long ReadInt64( this MemoryBus bus, long address )
        {
            return BitConverter.ToInt64( bus.Read( address, sizeof( long ) ), 0 );
        }

        public static float ReadFloat(this MemoryBus bus, long address)
        {
            return BitConverter.ToSingle(bus.Read(address, sizeof(float)), 0);
        }
        public static double ReadDouble(this MemoryBus bus, long address)
        {
            return BitConverter.ToDouble(bus.Read(address, sizeof(double)), 0);
        }
        
        public static sbyte ReadSByte( this MemoryBus bus, long address )
        {
            return ( sbyte )bus.Read( address );
        }

        public static ushort ReadUInt16( this MemoryBus bus, long address )
        {
            return BitConverter.ToUInt16( bus.Read( address, sizeof( ushort ) ), 0 );
        }

        public static uint ReadUInt32( this MemoryBus bus, long address )
        {
            return BitConverter.ToUInt32( bus.Read( address, sizeof( uint ) ), 0 );
        }

        public static ulong ReadUInt64( this MemoryBus bus, long address )
        {
            return BitConverter.ToUInt64( bus.Read( address, sizeof( ulong ) ), 0 );
        }

        public static int Write( this MemoryBus bus, long address, byte[] data )
        {
            for ( int i = 0; i < data.Length; i++ )
            {
                bus.Write( address + i, data[i] );
            }

            return data.Length;
        }

        public static int Write( this MemoryBus bus, long address, sbyte value )
        {
            return bus.Write( address, BitConverter.GetBytes( value ) );
        }

        public static int Write( this MemoryBus bus, long address, short value )
        {
            return bus.Write( address, BitConverter.GetBytes( value ) );
        }

        public static int Write( this MemoryBus bus, long address, char value )
        {
            return bus.Write( address, BitConverter.GetBytes( value ) );
        }

        public static int Write( this MemoryBus bus, long address, int value )
        {
            return bus.Write( address, BitConverter.GetBytes( value ) );
        }

        public static int Write( this MemoryBus bus, long address, long value )
        {
            return bus.Write( address, BitConverter.GetBytes( value ) );
        }

        public static int Write( this MemoryBus bus, long address, ushort value )
        {
            return bus.Write( address, BitConverter.GetBytes( value ) );
        }

        public static int Write( this MemoryBus bus, long address, uint value )
        {
            return bus.Write( address, BitConverter.GetBytes( value ) );
        }

        public static int Write( this MemoryBus bus, long address, ulong value )
        {
            return bus.Write( address, BitConverter.GetBytes( value ) );
        }

        public static int Write( this MemoryBus bus, long address, float value )
        {
            return bus.Write( address, BitConverter.GetBytes( value ) );
        }

        public static int Write( this MemoryBus bus, long address, double value )
        {
            return bus.Write( address, BitConverter.GetBytes( value ) );
        }

        public static int WriteCString( this MemoryBus bus, long address, string value )
        {
            long current = address;

            int i = 0;

            while ( current < value.Length )
            {
                current += bus.Write( current, value[i] );
                i++;
            }

            bus.Write( address + value.Length, '\0' );
            current += sizeof( char );

            return ( int )( current - address );
        }

        public static int WriteLString( this MemoryBus bus, long address, string value )
        {
            long current = address;

            bus.Write( current, ( long )value.Length );
            current += sizeof( long );

            int i = 0;

            while ( current < value.Length )
            {
                current += bus.Write( current, value[i] );
                i++;
            }

            return ( int )( current - address );
        }

        #endregion

    }

}
