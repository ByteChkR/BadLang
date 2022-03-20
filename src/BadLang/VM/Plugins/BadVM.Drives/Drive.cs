namespace BadVM.Drives;

public class Drive : IDisposable
{

    public readonly int Size;
    private readonly Stream m_BaseStream;

    #region Public

    public Drive( Stream baseStream, int size )
    {
        m_BaseStream = baseStream;
        m_BaseStream.SetLength( size );
        Size = size;
    }

    public Drive( string path, int size ) : this( File.Open( path, FileMode.OpenOrCreate, FileAccess.ReadWrite ), size )
    {
    }

    public Drive( int size ) : this( new MemoryStream( size ), size )
    {
    }

    public void Dispose()
    {
        m_BaseStream.Dispose();
    }

    public byte Read( int address )
    {
        m_BaseStream.Position = address;

        return ( byte )m_BaseStream.ReadByte();
    }

    public short Read16( int address )
    {
        m_BaseStream.Position = address;
        byte[] data = new byte[sizeof( short )];
        m_BaseStream.Read( data, 0, sizeof( short ) );

        return BitConverter.ToInt16( data, 0 );
    }

    public int Read32( int address )
    {
        m_BaseStream.Position = address;
        byte[] data = new byte[sizeof( int )];
        m_BaseStream.Read( data, 0, sizeof( int ) );

        return BitConverter.ToInt32( data, 0 );
    }

    public long Read64( int address )
    {
        m_BaseStream.Position = address;
        byte[] data = new byte[sizeof( long )];
        m_BaseStream.Read( data, 0, sizeof( long ) );

        return BitConverter.ToInt64( data, 0 );
    }

    public void Write( int address, byte value )
    {
        m_BaseStream.Position = address;
        m_BaseStream.WriteByte( value );
    }

    public void Write( int address, short value )
    {
        m_BaseStream.Position = address;
        m_BaseStream.Write( BitConverter.GetBytes( value ), 0, sizeof( short ) );
    }

    public void Write( int address, int value )
    {
        m_BaseStream.Position = address;
        m_BaseStream.Write( BitConverter.GetBytes( value ), 0, sizeof( int ) );
    }

    public void Write( int address, long value )
    {
        m_BaseStream.Position = address;
        m_BaseStream.Write( BitConverter.GetBytes( value ), 0, sizeof( long ) );
    }

    #endregion

}
