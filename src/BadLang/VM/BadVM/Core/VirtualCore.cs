using BadVM.Interop;
using BadVM.Shared;
using BadVM.Shared.Memory;

namespace BadVM.Core
{

    public class VirtualCore
    {

        private int m_CoreId = -1;

        private readonly object m_Lock = new object();

        private long m_ProgramCounter;
        private readonly Stack < VirtualCoreStackEntry > m_CallStack = new Stack < VirtualCoreStackEntry >();

        private readonly long m_StackStart;

        private readonly long m_StackSize;
        private long m_StackPointer;
        private bool m_Halt;
        private bool m_Running;
        private readonly MemoryBus m_MemoryBus;

        private readonly VirtualCPU m_Cpu;

        public IEnumerable < VirtualCoreStackEntry > CallStack => m_CallStack;

        public MemoryBus MemoryBus => m_MemoryBus;

        public byte[] DebugData => m_MemoryBus.Read( m_StackPointer, 16 );

        public int CoreId
        {
            get
            {
                if ( m_CoreId == -1 )
                {
                    m_CoreId = m_Cpu.GetCoreId( this );
                }

                return m_CoreId;
            }
        }

        public bool Running
        {
            get
            {
                lock ( m_Lock )
                {
                    return m_Running;
                }
            }
        }

        public long ProgramCounter
        {
            get
            {
                lock ( m_Lock )
                {
                    return m_ProgramCounter;
                }
            }
        }

        public long StackPointer
        {
            get
            {
                lock ( m_Lock )
                {
                    return m_StackPointer;
                }
            }
        }

        public event Action < VirtualCore >? OnCycle;

        #region Public

        public VirtualCore( long stackStart, long stackSize, MemoryBus memoryBus, VirtualCPU cpu )
        {
            m_StackStart = stackStart;
            m_MemoryBus = memoryBus;
            m_StackPointer = stackStart;
            m_StackSize = stackSize;
            m_Cpu = cpu;
        }

        public void AllocStackPointer( int size )
        {
            m_StackPointer -= size;

            if ( m_StackPointer > m_StackStart )
            {
                throw new StackPointerException( $"Core {CoreId} stack overflow" );
            }

            if ( m_StackPointer < m_StackStart - m_StackSize )
            {
                throw new StackPointerException( $"Core {CoreId} stack underflow" );
            }
        }

        public IEnumerator < OpCode > CreateRoutine( long address )
        {
            if ( m_Running )
            {
                throw new InvalidOperationException( "Virtual machine is already running" );
            }

            m_Running = true;
            m_Halt = false;
            m_ProgramCounter = address;

            while ( !m_Halt )
            {
                yield return ExecuteInstruction();
            }

            m_Running = false;
            m_Halt = false;
        }

        public void FreeStackPointer( int size )
        {
            m_StackPointer += size;

            if ( m_StackPointer > m_StackStart )
            {
                throw new StackPointerException( $"Core {CoreId} stack overflow" );
            }

            if ( m_StackPointer < m_StackStart - m_StackSize )
            {
                throw new StackPointerException( $"Core {CoreId} stack underflow" );
            }
        }

        public void Halt()
        {
            m_Halt = true;
            m_CallStack.Clear();
        }

        #endregion

        #region Private

        private OpCode ExecuteInstruction()
        {
            OnCycle?.Invoke( this );
            OpCode op = ( OpCode )m_MemoryBus.ReadUInt16( m_ProgramCounter );
            m_ProgramCounter += sizeof( ushort );

            switch ( op )
            {
                case OpCode.Nop:
                {
                    break;
                }

                case OpCode.Halt:
                {
                    Halt();

                    break;
                }

                case OpCode.InitCore:
                {
                    byte coreId = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    m_Cpu.InitCore( coreId, addr );

                    break;
                }

                case OpCode.AbortCore:
                {
                    byte coreId = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    m_Cpu.AbortCore( coreId );

                    break;
                }

                case OpCode.CoreStatus:
                {
                    byte coreId = m_MemoryBus.Read( m_StackPointer );
                    m_MemoryBus.Write( m_StackPointer, m_Cpu.GetCore( coreId ).Running ? ( byte )1 : ( byte )0 );

                    break;
                }

                case OpCode.CoreCount:
                {
                    AllocStackPointer( sizeof( byte ) );
                    m_MemoryBus.Write( m_StackPointer, ( byte )m_Cpu.GetCoreCount() );

                    break;
                }

                case OpCode.SwapI8:
                {
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    byte b = m_MemoryBus.Read( m_StackPointer + sizeof( byte ) );

                    m_MemoryBus.Write( m_StackPointer, b );
                    m_MemoryBus.Write( m_StackPointer + sizeof( byte ), a );

                    break;
                }

                case OpCode.SwapI16:
                {
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    short b = m_MemoryBus.ReadInt16( m_StackPointer + sizeof( short ) );

                    m_MemoryBus.Write( m_StackPointer, b );
                    m_MemoryBus.Write( m_StackPointer + sizeof( short ), a );

                    break;
                }

                case OpCode.SwapI32:
                {
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    int b = m_MemoryBus.ReadInt32( m_StackPointer + sizeof( int ) );

                    m_MemoryBus.Write( m_StackPointer, b );
                    m_MemoryBus.Write( m_StackPointer + sizeof( int ), a );

                    break;
                }

                case OpCode.SwapI64:
                {
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    long b = m_MemoryBus.ReadInt64( m_StackPointer + sizeof( long ) );

                    m_MemoryBus.Write( m_StackPointer, b );
                    m_MemoryBus.Write( m_StackPointer + sizeof( long ), a );

                    break;
                }

                case OpCode.DupI8:
                {
                    byte value = m_MemoryBus.Read( m_StackPointer );
                    AllocStackPointer( sizeof( byte ) );
                    m_MemoryBus.Write( m_StackPointer, value );

                    break;
                }

                case OpCode.DupI16:
                {
                    short value = m_MemoryBus.ReadInt16( m_StackPointer );
                    AllocStackPointer( sizeof( short ) );
                    m_MemoryBus.Write( m_StackPointer, value );

                    break;
                }

                case OpCode.DupI32:
                {
                    int value = m_MemoryBus.ReadInt32( m_StackPointer );
                    AllocStackPointer( sizeof( int ) );
                    m_MemoryBus.Write( m_StackPointer, value );

                    break;
                }

                case OpCode.DupI64:
                {
                    long value = m_MemoryBus.ReadInt64( m_StackPointer );
                    AllocStackPointer( sizeof( long ) );
                    m_MemoryBus.Write( m_StackPointer, value );

                    break;
                }

                case OpCode.PushI64:
                {
                    long value = m_MemoryBus.ReadInt64( m_ProgramCounter );
                    m_ProgramCounter += sizeof( long );
                    AllocStackPointer( sizeof( long ) );
                    m_MemoryBus.Write( m_StackPointer, value );

                    break;
                }

                case OpCode.PushI32:
                {
                    int value = m_MemoryBus.ReadInt32( m_ProgramCounter );
                    m_ProgramCounter += sizeof( int );
                    AllocStackPointer( sizeof( int ) );
                    m_MemoryBus.Write( m_StackPointer, value );

                    break;
                }

                case OpCode.PushI16:
                {
                    short value = m_MemoryBus.ReadInt16( m_ProgramCounter );
                    m_ProgramCounter += sizeof( short );
                    AllocStackPointer( sizeof( short ) );
                    m_MemoryBus.Write( m_StackPointer, value );

                    break;
                }

                case OpCode.PushI8:
                {
                    byte value = m_MemoryBus.Read( m_ProgramCounter );
                    m_ProgramCounter += sizeof( byte );
                    AllocStackPointer( sizeof( byte ) );
                    m_MemoryBus.Write( m_StackPointer, value );

                    break;
                }

                case OpCode.PushSP:
                {
                    long sp = m_StackPointer;
                    AllocStackPointer( sizeof( long ) );
                    m_MemoryBus.Write( m_StackPointer, sp );

                    break;
                }

                case OpCode.PushSF:
                {
                    AllocStackPointer( sizeof( long ) );

                    if ( m_CallStack.Count != 0 )
                    {
                        m_MemoryBus.Write( m_StackPointer, m_CallStack.Peek().StackFrameAddress );
                    }
                    else
                    {
                        m_MemoryBus.Write( m_StackPointer, m_StackStart );
                    }

                    break;
                }

                case OpCode.LoadI64:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    long v = m_MemoryBus.ReadInt64( addr );
                    m_MemoryBus.Write( m_StackPointer, v );

                    break;
                }

                case OpCode.LoadI32:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    AllocStackPointer( sizeof( int ) );
                    int v = m_MemoryBus.ReadInt32( addr );
                    m_MemoryBus.Write( m_StackPointer, v );

                    break;
                }

                case OpCode.LoadI16:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    AllocStackPointer( sizeof( short ) );
                    m_MemoryBus.Write( m_StackPointer, m_MemoryBus.ReadInt16( addr ) );

                    break;
                }

                case OpCode.LoadI8:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    AllocStackPointer( sizeof( byte ) );
                    byte v = m_MemoryBus.Read( addr );
                    m_MemoryBus.Write( m_StackPointer, v );

                    break;
                }

                case OpCode.LoadN:
                {
                    long size = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    AllocStackPointer( ( int )size );
                    m_MemoryBus.Copy( addr, m_StackPointer, ( int )size );

                    break;
                }

                case OpCode.StoreI64:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    m_MemoryBus.Write( addr, m_MemoryBus.ReadInt64( m_StackPointer ) );
                    FreeStackPointer( sizeof( long ) );

                    break;
                }

                case OpCode.StoreI32:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    int v = m_MemoryBus.ReadInt32( m_StackPointer );
                    m_MemoryBus.Write( addr, v );
                    FreeStackPointer( sizeof( int ) );

                    break;
                }

                case OpCode.StoreI16:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    m_MemoryBus.Write( addr, m_MemoryBus.ReadInt16( m_StackPointer ) );
                    FreeStackPointer( sizeof( short ) );

                    break;
                }

                case OpCode.StoreI8:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    m_MemoryBus.Write( addr, m_MemoryBus.Read( m_StackPointer ) );
                    FreeStackPointer( sizeof( byte ) );

                    break;
                }

                case OpCode.StoreN:
                {
                    long size = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    m_MemoryBus.Copy( m_StackPointer, addr, ( int )size );
                    FreeStackPointer( ( int )size );

                    break;
                }

                case OpCode.Pop64:
                    FreeStackPointer( sizeof( long ) );

                    break;

                case OpCode.Pop32:
                    FreeStackPointer( sizeof( int ) );

                    break;

                case OpCode.Pop16:
                    FreeStackPointer( sizeof( short ) );

                    break;

                case OpCode.Pop8:
                    FreeStackPointer( sizeof( byte ) );

                    break;

                case OpCode.MovSP:
                {
                    int off = m_MemoryBus.ReadInt32( m_ProgramCounter );
                    m_ProgramCounter += sizeof( int );
                    FreeStackPointer( off );

                    break;
                }

                case OpCode.MovSFI8:
                {
                    int off = m_MemoryBus.ReadInt32( m_ProgramCounter );
                    m_ProgramCounter += sizeof( int );

                    byte a = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    m_MemoryBus.Write( m_CallStack.Peek().StackFrameAddress + off, a );

                    break;
                }

                case OpCode.MovSFI16:
                {
                    int off = m_MemoryBus.ReadInt32( m_ProgramCounter );
                    m_ProgramCounter += sizeof( int );

                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    m_MemoryBus.Write( m_CallStack.Peek().StackFrameAddress + off, a );

                    break;
                }

                case OpCode.MovSFI32:
                {
                    int off = m_MemoryBus.ReadInt32( m_ProgramCounter );
                    m_ProgramCounter += sizeof( int );

                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    m_MemoryBus.Write( m_CallStack.Peek().StackFrameAddress + off, a );

                    break;
                }

                case OpCode.MovSFI64:
                {
                    int off = m_MemoryBus.ReadInt32( m_ProgramCounter );
                    m_ProgramCounter += sizeof( int );

                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    m_MemoryBus.Write( m_CallStack.Peek().StackFrameAddress + off, a );

                    break;
                }

                case OpCode.AddI8:
                {
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte b = m_MemoryBus.Read( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( byte )( a + b ) );

                    break;
                }

                case OpCode.AddI16:
                {
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( short )( a + b ) );

                    break;
                }

                case OpCode.AddI32:
                {
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a + b );

                    break;
                }

                case OpCode.AddI64:
                {
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a + b );

                    break;
                }

                case OpCode.SubI8:
                {
                    byte b = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte a = m_MemoryBus.Read( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( byte )( a - b ) );

                    break;
                }

                case OpCode.SubI16:
                {
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( short )( a - b ) );

                    break;
                }

                case OpCode.SubI32:
                {
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a - b );

                    break;
                }

                case OpCode.SubI64:
                {
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a - b );

                    break;
                }

                case OpCode.MulI8:
                {
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte b = m_MemoryBus.Read( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( byte )( a * b ) );

                    break;
                }

                case OpCode.MulI16:
                {
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( short )( a * b ) );

                    break;
                }

                case OpCode.MulI32:
                {
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a * b );

                    break;
                }

                case OpCode.MulI64:
                {
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a * b );

                    break;
                }

                case OpCode.DivI8:
                {
                    byte b = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte a = m_MemoryBus.Read( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( byte )( a / b ) );

                    break;
                }

                case OpCode.DivI16:
                {
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( short )( a / b ) );

                    break;
                }

                case OpCode.DivI32:
                {
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a / b );

                    break;
                }

                case OpCode.DivI64:
                {
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a / b );

                    break;
                }

                case OpCode.ModI8:
                {
                    byte b = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte a = m_MemoryBus.Read( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( byte )( a % b ) );

                    break;
                }

                case OpCode.ModI16:
                {
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( short )( a % b ) );

                    break;
                }

                case OpCode.ModI32:
                {
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a % b );

                    break;
                }

                case OpCode.ModI64:
                {
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a % b );

                    break;
                }

                case OpCode.AndI8:
                {
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte b = m_MemoryBus.Read( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( byte )( a & b ) );

                    break;
                }

                case OpCode.AndI16:
                {
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( short )( a & b ) );

                    break;
                }

                case OpCode.AndI32:
                {
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a & b );

                    break;
                }

                case OpCode.AndI64:
                {
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a & b );

                    break;
                }

                case OpCode.OrI8:
                {
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte b = m_MemoryBus.Read( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( byte )( a | b ) );

                    break;
                }

                case OpCode.OrI16:

                {
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( short )( a | b ) );

                    break;
                }

                case OpCode.OrI32:
                {
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a | b );

                    break;
                }

                case OpCode.OrI64:
                {
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a | b );

                    break;
                }

                case OpCode.XorI8:
                {
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte b = m_MemoryBus.Read( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( byte )( a ^ b ) );

                    break;
                }

                case OpCode.XorI16:
                {
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( short )( a ^ b ) );

                    break;
                }

                case OpCode.XorI32:
                {
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a ^ b );

                    break;
                }

                case OpCode.XorI64:
                {
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a ^ b );

                    break;
                }

                case OpCode.NotI8:
                {
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    m_MemoryBus.Write( m_StackPointer, ( byte )~a );

                    break;
                }

                case OpCode.NotI16:
                {
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    m_MemoryBus.Write( m_StackPointer, ( short )~a );

                    break;
                }

                case OpCode.NotI32:
                {
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    m_MemoryBus.Write( m_StackPointer, ~a );

                    break;
                }

                case OpCode.NotI64:
                {
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    m_MemoryBus.Write( m_StackPointer, ~a );

                    break;
                }

                case OpCode.ShlI8:
                {
                    byte b = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte a = m_MemoryBus.Read( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( byte )( a << b ) );

                    break;
                }

                case OpCode.ShlI16:
                {
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( short )( a << b ) );

                    break;
                }

                case OpCode.ShlI32:
                {
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a << b );

                    break;
                }

                case OpCode.ShlI64:
                {
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a << ( int )b );

                    break;
                }

                case OpCode.ShrI8:
                {
                    byte b = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte a = m_MemoryBus.Read( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( byte )( a >> b ) );

                    break;
                }

                case OpCode.ShrI16:
                {
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, ( short )( a >> b ) );

                    break;
                }

                case OpCode.ShrI32:
                {
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a >> b );

                    break;
                }

                case OpCode.ShrI64:
                {
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );

                    m_MemoryBus.Write( m_StackPointer, a >> ( int )b );

                    break;
                }

                case OpCode.Call:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );

                    m_CallStack.Push( new VirtualCoreStackEntry( m_ProgramCounter, m_StackPointer ) );
                    m_ProgramCounter = addr;

                    break;
                }

                case OpCode.Return:
                {
                    VirtualCoreStackEntry e = m_CallStack.Pop();
                    m_ProgramCounter = e.ReturnAddress;

                    byte r = m_MemoryBus.Read( m_StackPointer );

                    break;
                }

                case OpCode.Jump:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );

                    m_ProgramCounter = addr;

                    break;
                }

                case OpCode.BranchZeroI8:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    byte value = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );

                    if ( value == 0 )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchZeroI16:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    short value = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );

                    if ( value == 0 )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchZeroI32:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    int value = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );

                    if ( value == 0 )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchZeroI64:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long value = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );

                    if ( value == 0 )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchNotZeroI8:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    byte value = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );

                    if ( value != 0 )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchNotZeroI16:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    short value = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );

                    if ( value != 0 )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchNotZeroI32:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    int value = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );

                    if ( value != 0 )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchNotZeroI64:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long value = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );

                    if ( value != 0 )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchLessI8:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    byte b = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );

                    if ( a < b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchLessI16:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );

                    if ( a < b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchLessI32:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );

                    if ( a < b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchLessI64:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );

                    if ( a < b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchGreaterI8:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    byte b = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );

                    if ( a > b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchGreaterI16:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );

                    if ( a > b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchGreaterI32:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );

                    if ( a > b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchGreaterI64:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );

                    if ( a > b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchLessOrEqualI8:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    byte b = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );

                    if ( a <= b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchLessOrEqualI16:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );

                    if ( a <= b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchLessOrEqualI32:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );

                    if ( a <= b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchLessOrEqualI64:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );

                    if ( a <= b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchGreaterOrEqualI8:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    byte b = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );
                    byte a = m_MemoryBus.Read( m_StackPointer );
                    FreeStackPointer( sizeof( byte ) );

                    if ( a >= b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchGreaterOrEqualI16:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    short b = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );
                    short a = m_MemoryBus.ReadInt16( m_StackPointer );
                    FreeStackPointer( sizeof( short ) );

                    if ( a >= b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchGreaterOrEqualI32:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    int b = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );
                    int a = m_MemoryBus.ReadInt32( m_StackPointer );
                    FreeStackPointer( sizeof( int ) );

                    if ( a >= b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.BranchGreaterOrEqualI64:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long b = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );
                    long a = m_MemoryBus.ReadInt64( m_StackPointer );
                    FreeStackPointer( sizeof( long ) );

                    if ( a >= b )
                    {
                        m_ProgramCounter = addr;
                    }

                    break;
                }

                case OpCode.InteropResolve:
                {
                    long addr = m_MemoryBus.ReadInt64( m_StackPointer );

                    List < char > name = new List < char >();

                    char current = ( char )m_MemoryBus.ReadInt16( addr );

                    while ( current != '\0' )
                    {
                        name.Add( current );
                        addr += sizeof( char );
                        current = ( char )m_MemoryBus.ReadInt16( addr );
                    }

                    string n = new(name.ToArray());
                    m_MemoryBus.Write( m_StackPointer, InteropHelper.Resolve( n ) );

                    break;
                }

                case OpCode.InteropCall:
                {
                    InteropHelper.Call( this, m_MemoryBus );

                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            return op;
        }

        #endregion

    }

}
