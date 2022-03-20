namespace BadVM.Debugger;

public class DebuggerHost
{

    private readonly List < IDebugger > m_Debuggers = new List < IDebugger >();

    #region Public

    public void AddDebugger( IDebugger debugger )
    {
        m_Debuggers.Add( debugger );
    }

    public IDebugger? GetDebugger( string? name = null )
    {
        if ( name == null )
        {
            return m_Debuggers.FirstOrDefault();
        }

        return m_Debuggers.FirstOrDefault( d => d.Name == name );
    }

    #endregion

}
