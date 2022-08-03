namespace LF.Compiler.Logging;

public interface ILogger
{
    public void Log(object obj, LogLevel level);
}