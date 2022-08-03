namespace LF.Compiler.Logging;

public static class Logger
{
    private static ILogger m_Logger = new VoidLogger();

    public static void SetLogger(ILogger? logger)
    {
        m_Logger = logger ?? new VoidLogger();
    }

    public static void Log(object obj, LogLevel level)
    {
        m_Logger.Log(obj, level);
    }

    public static void Debug(object obj)
    {
#if DEBUG
        m_Logger.Debug(obj);
#endif
    }

    public static void Info(object obj)
    {
        m_Logger.Info(obj);
    }

    public static void Warning(object obj)
    {
        m_Logger.Warning(obj);
    }

    public static void Error(object obj)
    {
        m_Logger.Error(obj);
    }
}