using BadVM.Core;
using BadVM.Shared.Memory;

namespace BadVM.Interop.Internal;

internal static class InteropConverters
{

    private static readonly Dictionary < Type, Func < object, byte[] > > m_WriteConverters =
        new Dictionary < Type, Func < object, byte[] > >
        {
            { typeof( byte ), WriteByte },
            { typeof( ushort ), WriteUShort },
            { typeof( char ), WriteChar },
            { typeof( uint ), WriteUInt },
            { typeof( ulong ), WriteULong },
            { typeof( sbyte ), WriteByte },
            { typeof( short ), WriteShort },
            { typeof( int ), WriteInt },
            { typeof( long ), WriteLong },
            { typeof( float ), WriteFloat },
            { typeof( double ), WriteDouble },
        };

    private static readonly Dictionary < Type, Func < VirtualCore, MemoryBus, object > > m_ReadConverters =
        new Dictionary < Type, Func < VirtualCore, MemoryBus, object > >
        {
            { typeof( byte ), ReadByte },
            { typeof( ushort ), ReadUShort },
            { typeof( char ), ReadChar },
            { typeof( uint ), ReadUInt },
            { typeof( ulong ), ReadULong },
            { typeof( sbyte ), ReadSByte },
            { typeof( short ), ReadShort },
            { typeof( int ), ReadInt },
            { typeof( long ), ReadLong },
            { typeof( string ), ReadString },
            { typeof( float ), ReadFloat },
            { typeof( double ), ReadDouble },
        };

    #region Public

    public static T Read < T >( VirtualCore core, MemoryBus bus )
    {
        return ( T )Read( core, bus, typeof( T ) );
    }

    public static object Read( VirtualCore core, MemoryBus bus, Type type )
    {
        return m_ReadConverters[type]( core, bus );
    }

    public static object ReadByte( VirtualCore core, MemoryBus bus )
    {
        byte b = bus.Read( core.StackPointer );
        core.FreeStackPointer( sizeof( byte ) );

        return b;
    }

    public static object ReadChar( VirtualCore core, MemoryBus bus )
    {
        char s = ( char )bus.ReadUInt16( core.StackPointer );
        core.FreeStackPointer( sizeof( char ) );

        return s;
    }

    public static object ReadInt( VirtualCore core, MemoryBus bus )
    {
        int i = bus.ReadInt32( core.StackPointer );
        core.FreeStackPointer( sizeof( int ) );

        return i;
    }

    public static object ReadLong( VirtualCore core, MemoryBus bus )
    {
        long l = bus.ReadInt64( core.StackPointer );
        core.FreeStackPointer( sizeof( long ) );

        return l;
    }

    public static object ReadFloat(VirtualCore core, MemoryBus bus)
    {
        float l = bus.ReadFloat(core.StackPointer);
        core.FreeStackPointer(sizeof(float));
        return l;
    }
    
    public static object ReadDouble(VirtualCore core, MemoryBus bus)
    {
        double l = bus.ReadDouble(core.StackPointer);
        core.FreeStackPointer(sizeof(double));
        return l;
    }

    public static object ReadSByte( VirtualCore core, MemoryBus bus )
    {
        byte b = bus.Read( core.StackPointer );
        core.FreeStackPointer( sizeof( sbyte ) );

        return b;
    }

    public static object ReadShort( VirtualCore core, MemoryBus bus )
    {
        short s = bus.ReadInt16( core.StackPointer );
        core.FreeStackPointer( sizeof( short ) );

        return s;
    }

    public static object ReadString( VirtualCore core, MemoryBus bus )
    {
        long addr = bus.ReadInt64( core.StackPointer );
        core.FreeStackPointer( sizeof( long ) );

        List < char > name = new List < char >();

        char current = ( char )bus.ReadInt16( addr );

        while ( current != '\0' )
        {
            name.Add( current );
            addr += sizeof( char );
            current = ( char )bus.ReadInt16( addr );
        }

        return new string( name.ToArray() );
    }

    public static object ReadUInt( VirtualCore core, MemoryBus bus )
    {
        uint i = bus.ReadUInt32( core.StackPointer );
        core.FreeStackPointer( sizeof( uint ) );

        return i;
    }

    public static object ReadULong( VirtualCore core, MemoryBus bus )
    {
        ulong l = bus.ReadUInt64( core.StackPointer );
        core.FreeStackPointer( sizeof( ulong ) );

        return l;
    }

    public static object ReadUShort( VirtualCore core, MemoryBus bus )
    {
        ushort s = bus.ReadUInt16( core.StackPointer );
        core.FreeStackPointer( sizeof( ushort ) );

        return s;
    }

    public static void Write( VirtualCore core, MemoryBus bus, object o )
    {
        byte[] data = m_WriteConverters[o.GetType()]( o );
        core.AllocStackPointer( data.Length );
        bus.Write( core.StackPointer, data );
    }

    public static byte[] WriteChar( object value )
    {
        return BitConverter.GetBytes( ( char )value );
    }

    public static byte[] WriteInt( object value )
    {
        return BitConverter.GetBytes( ( int )value );
    }

    public static byte[] WriteLong( object value )
    {
        return BitConverter.GetBytes( ( long )value );
    }
    
    public static byte[] WriteFloat(object value)
    {
        return BitConverter.GetBytes((float)value);
    }
    
    public static byte[] WriteDouble(object value)
    {
        return BitConverter.GetBytes((double)value);
    }

    public static byte[] WriteShort( object value )
    {
        return BitConverter.GetBytes( ( short )value );
    }

    public static byte[] WriteUInt( object value )
    {
        return BitConverter.GetBytes( ( uint )value );
    }

    public static byte[] WriteULong( object value )
    {
        return BitConverter.GetBytes( ( ulong )value );
    }

    public static byte[] WriteUShort( object value )
    {
        return BitConverter.GetBytes( ( ushort )value );
    }

    #endregion

    #region Private

    private static byte[] WriteByte( object value )
    {
        return new [] { ( byte )value };
    }

    #endregion

}
