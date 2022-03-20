using BadVM.Core;
using BadVM.Settings;
using BadVM.Shared.Logging;
using BadVM.Shared.Memory;

namespace BadVM
{

    public class VirtualCPU
    {

        public static readonly LogMask LogMask = new LogMask( nameof( VirtualCPU ) );
        private readonly VirtualCPUThread[] m_Threads;
        private readonly MemoryBus m_Bus;

        public event Action < VirtualCore >? OnCoreCycle;

        public bool IsRunning => m_Threads.All( x => x.AnyActiveCores );

        #region Unity Event Functions

        public void Start()
        {
            LogMask.LogMessage( "Starting virtual CPU" );

            foreach ( VirtualCPUThread thread in m_Threads )
            {
                thread.Start();
            }
        }

        #endregion

        #region Public

        public VirtualCPU( BadLangSettings settings, MemoryBus bus )
        {
            m_Bus = bus;

            List < VirtualCPUThread > threads = new List < VirtualCPUThread >();
            int stackStart = settings.StackStart + settings.StackSize;

            for ( int i = 0; i < settings.ThreadCount; i++ )
            {
                List < VirtualCore > cores = new List < VirtualCore >();

                for ( int j = 0; j < settings.HyperThreadCount; j++ )
                {
                    VirtualCore core = new VirtualCore( stackStart, settings.StackSize, bus, this );
                    cores.Add( core );
                    core.OnCycle += c => OnCoreCycle?.Invoke( c );
                    stackStart += settings.StackSize;
                }

                threads.Add( new VirtualCPUThread( cores.ToArray() ) );
            }

            m_Threads = threads.ToArray();
        }

        public void AbortCore( int index )
        {
            int current = 0;

            foreach ( VirtualCPUThread thread in m_Threads )
            {
                if ( current <= index && index < current + thread.CoreCount )
                {
                    thread.AbortCore( index - current );

                    return;
                }

                current += thread.CoreCount;
            }

            throw new IndexOutOfRangeException();
        }

        public void Exit()
        {
            foreach ( VirtualCPUThread thread in m_Threads )
            {
                thread.Exit();
            }
        }

        public VirtualCore GetCore( int index )
        {
            int current = 0;

            foreach ( VirtualCPUThread thread in m_Threads )
            {
                if ( current <= index && index < current + thread.CoreCount )
                {
                    return thread.GetCore( index - current );
                }

                current += thread.CoreCount;
            }

            throw new IndexOutOfRangeException();
        }

        public int GetCoreCount()
        {
            return m_Threads.Sum( x => x.CoreCount );
        }

        public int GetThreadCount()
        {
            return m_Threads.Length;
        }

        public void InitCore( int index, long address )
        {
            int current = 0;

            foreach ( VirtualCPUThread thread in m_Threads )
            {
                if ( current <= index && index < current + thread.CoreCount )
                {
                    thread.InitCore( index - current, address );

                    return;
                }

                current += thread.CoreCount;
            }

            throw new IndexOutOfRangeException();
        }

        public void StartSynchronous( int synchronousThread = 0 )
        {
            LogMask.LogMessage( "Starting virtual CPU" );

            for ( int i = 0; i < m_Threads.Length; i++ )
            {
                if ( i == synchronousThread )
                {
                    continue;
                }

                VirtualCPUThread thread = m_Threads[i];
                thread.Start();
            }

            m_Threads[synchronousThread].StartSynchronous();
        }

        internal int GetCoreId( VirtualCore core )
        {
            int current = 0;

            foreach ( VirtualCPUThread thread in m_Threads )
            {
                for ( int i = 0; i < thread.CoreCount; i++ )
                {
                    if ( thread.GetCore( i ) == core )
                    {
                        return current + i;
                    }
                }

                current += thread.CoreCount;
            }

            throw new InvalidOperationException( "Core is not found" );
        }

        #endregion

    }

}
