using BadVM.Core;
using BadVM.Shared;

namespace BadVM
{

    public class VirtualCPUThread
    {

        private Thread? m_Thread;
        private bool m_Exit;
        private bool m_IsRunning;

        private readonly object m_ThreadLock = new object();
        private readonly VirtualCore[] m_Cores;
        private readonly IEnumerator < OpCode >?[] m_CoreRoutines;

        public bool AnyActiveCores => m_Cores.Any( x => x.Running );

        public int CoreCount => m_Cores.Length;

        #region Unity Event Functions

        public void Start()
        {
            lock ( m_ThreadLock )
            {
                if ( m_IsRunning )
                {
                    throw new InvalidOperationException( "Thread is already running" );
                }

                m_IsRunning = true;
                m_Exit = false;
                m_Thread = new Thread( Run );
                m_Thread.Start();
            }
        }

        #endregion

        #region Public

        public VirtualCPUThread( VirtualCore[] cores )
        {
            m_Cores = cores.ToArray();
            m_CoreRoutines = new IEnumerator < OpCode >[m_Cores.Length];
        }

        public void AbortCore( int index )
        {
            lock ( m_ThreadLock )
            {
                if ( m_CoreRoutines[index] == null )
                {
                    throw new InvalidOperationException( "Core is not initialized" );
                }

                m_Cores[index].Halt();
            }
        }

        public void Exit()
        {
            lock ( m_ThreadLock )
            {
                m_Exit = true;
            }
        }

        public VirtualCore GetCore( int index )
        {
            return m_Cores[index];
        }

        public void InitCore( int index, long address )
        {
            lock ( m_ThreadLock )
            {
                if ( m_CoreRoutines[index] != null )
                {
                    throw new InvalidOperationException( "Core is already initialized" );
                }

                m_CoreRoutines[index] = m_Cores[index].CreateRoutine( address );
            }
        }

        public void StartSynchronous()
        {
            lock ( m_ThreadLock )
            {
                if ( m_IsRunning )
                {
                    throw new InvalidOperationException( "Thread is already running" );
                }

                m_IsRunning = true;
                m_Exit = false;
            }

            Run();
        }

        #endregion

        #region Private

        private void Run()
        {
            while ( true )
            {
                lock ( m_ThreadLock )
                {
                    if ( m_Exit || m_CoreRoutines.All( x => x == null ) )
                    {
                        m_Thread = null;
                        m_IsRunning = false;

                        return;
                    }
                }

                for ( int i = 0; i < m_CoreRoutines.Length; i++ )
                {
                    IEnumerator < OpCode >? e;

                    lock ( m_ThreadLock )
                    {
                        e = m_CoreRoutines[i];
                    }

                    if ( e == null )
                    {
                        continue;
                    }

                    if ( !e.MoveNext() )
                    {
                        lock ( m_ThreadLock )
                        {
                            m_CoreRoutines[i] = null;
                        }
                    }
                }
            }
        }

        #endregion

    }

}
