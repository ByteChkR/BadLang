namespace BadVM.Shared.Logging;

public class LogMask
{

    private readonly string[] LogPath;

    #region Public

    public LogMask( string name ) : this( new string[] { name } )
    {
    }

    public LogMask CreateChild( string name )
    {
        return new LogMask( LogPath.Concat( new string[] { name } ).ToArray() );
    }

    public void Info( string message )
    {
        Log.Info( message, this );
    }

    public void LogMessage( string message )
    {
        Log.LogMessage( message, this );
    }

    public override string ToString()
    {
        return string.Join( ".", LogPath );
    }

    public void Warning( string message )
    {
        Log.Warning( message, this );
    }

    #endregion

    #region Private

    private LogMask( string[] logPath )
    {
        LogPath = logPath;
    }

    #endregion

}
