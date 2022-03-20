using BadVM.Shared.Logging;

namespace BadAssembler;

public class PostSegmentParseTasks
{

    public static readonly LogMask LogMask = new LogMask( "PostSegmentParseTasks" );

    private readonly struct ParseTask
    {

        public readonly Action Task;
        public readonly string Name;

        public ParseTask( string name, Action task )
        {
            Name = name;
            Task = task;
        }

    }

    private readonly Queue < ParseTask > m_Tasks = new Queue < ParseTask >();

    #region Public

    public void AddTask( string name, Action task )
    {
        m_Tasks.Enqueue( new ParseTask( name, task ) );
    }

    public void Run()
    {
        int processed = 0;
        while ( m_Tasks.Count != 0 )
        {
            ParseTask task = m_Tasks.Dequeue();
            processed++;
            LogMask.Info( $"[{processed}/{processed+m_Tasks.Count}] Running Task: {task.Name}" );
            task.Task();
        }
    }

    #endregion

}
