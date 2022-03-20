namespace BadVM.Shared.Logging;

public class Log
{

    public readonly string Message;
    public readonly LogMask LogMask;
    public readonly LogType Type;

    private static readonly List < ILogger > s_Loggers = new List < ILogger >();

    #region Public

    public static void AddLogger( ILogger logger )
    {
        if ( !s_Loggers.Contains( logger ) )
        {
            s_Loggers.Add( logger );
        }
    }

    public static void Info( string message, LogMask mask )
    {
        LogMessage( new Log( message, LogType.Info, mask ) );
    }

    public static void LogMessage( string message, LogMask mask )
    {
        LogMessage( new Log( message, LogType.Log, mask ) );
    }

    public static void Warning( string message, LogMask mask )
    {
        LogMessage( new Log( message, LogType.Warning, mask ) );
    }

    #endregion

    #region Private

    private Log( string message, LogType type, LogMask logMask )
    {
        Message = message;
        Type = type;
        LogMask = logMask;
    }

    private static void LogMessage( Log log )
    {
        foreach ( ILogger logger in s_Loggers )
        {
            logger.Log( log );
        }
    }

    #endregion

}
