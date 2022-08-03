namespace LF.Compiler.Logging;

public class TagLogger : ILogger
{
    private readonly ILogger m_Logger;
    private readonly string? m_Tag;

    public TagLogger(ILogger logger, string? tag = null)
    {
        m_Logger = logger;
        m_Tag = tag;
    }

    public void Log(object obj, LogLevel level)
    {
        m_Logger.Log($"[{level}]{$"[{m_Tag}]" ?? ""} {obj}", level);
    }
}