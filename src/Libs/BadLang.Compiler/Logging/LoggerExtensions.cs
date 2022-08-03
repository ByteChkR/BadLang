namespace LF.Compiler.Logging;

public static class LoggerExtensions
{
    public static void Debug(this ILogger logger, object obj)
    {
        logger.Log(obj, LogLevel.Debug);
    }

    public static void Info(this ILogger logger, object obj)
    {
        logger.Log(obj, LogLevel.Info);
    }

    public static void Warning(this ILogger logger, object obj)
    {
        logger.Log(obj, LogLevel.Warning);
    }

    public static void Error(this ILogger logger, object obj)
    {
        logger.Log(obj, LogLevel.Error);
    }
}