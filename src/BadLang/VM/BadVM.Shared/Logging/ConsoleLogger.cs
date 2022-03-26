namespace BadVM.Shared.Logging;

public class ConsoleLogger : ILogger
{

    public static ConsoleColor InfoColor { get;} = ConsoleColor.White;
    public static ConsoleColor LogColor { get; } = ConsoleColor.DarkGray;
    public static ConsoleColor WarningColor { get; } = ConsoleColor.Yellow;

    // public static bool operator ==( ConsoleLogger? left, ConsoleLogger? right )
    // {
    //     return Equals( left, right );
    // }
    //
    // public static bool operator !=( ConsoleLogger? left, ConsoleLogger? right )
    // {
    //     return !Equals( left, right );
    // }

    #region Public

    public override bool Equals( object obj )
    {
        return obj is ConsoleLogger;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public void Log( Log message )
    {
        switch ( message.Type )
        {
            case LogType.Info:
                Write( $"[{message.Type}][{message.LogMask}] {message.Message}", InfoColor );

                break;

            case LogType.Log:
                Write( $"[{message.Type}][{message.LogMask}] {message.Message}", LogColor );

                break;

            case LogType.Warning:
                Write( $"[{message.Type}][{message.LogMask}] {message.Message}", WarningColor );

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
