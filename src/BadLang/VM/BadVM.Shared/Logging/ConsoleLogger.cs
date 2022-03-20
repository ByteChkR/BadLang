namespace BadVM.Shared.Logging;

public class ConsoleLogger : ILogger
{

    public ConsoleColor InfoColor = ConsoleColor.White;
    public ConsoleColor LogColor = ConsoleColor.DarkGray;
    public ConsoleColor WarningColor = ConsoleColor.Yellow;

    public static bool operator ==( ConsoleLogger? left, ConsoleLogger? right )
    {
        return Equals( left, right );
    }

    public static bool operator !=( ConsoleLogger? left, ConsoleLogger? right )
    {
        return !Equals( left, right );
    }

    #region Public

    public override bool Equals( object obj )
    {
        return obj is ConsoleLogger;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public void Log( Log log )
    {
        switch ( log.Type )
        {
            case LogType.Info:
                Write( $"[{log.Type}][{log.LogMask}] {log.Message}", InfoColor );

                break;

            case LogType.Log:
                Write( $"[{log.Type}][{log.LogMask}] {log.Message}", LogColor );

                break;

            case LogType.Warning:
                Write( $"[{log.Type}][{log.LogMask}] {log.Message}", WarningColor );

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region Protected

    protected bool Equals( ConsoleLogger other )
    {
        return true;
    }

    #endregion

    #region Private

    private static void Write( string message, ConsoleColor color )
    {
        ConsoleColor c = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine( message );
        Console.ForegroundColor = c;
    }

    #endregion

}
