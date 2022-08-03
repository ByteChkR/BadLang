using LF.Compiler.Logging;

namespace LFC;

internal class LFLogger : ILogger
{
    public void Log(object obj, LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Debug:
                Write(obj, ConsoleColor.Blue, ConsoleColor.Black);

                break;
            case LogLevel.Info:
                Write(obj, ConsoleColor.Cyan, ConsoleColor.Black);

                break;
            case LogLevel.Warning:
                Write(obj, ConsoleColor.Yellow, ConsoleColor.Black);

                break;
            case LogLevel.Error:
                Write(obj, ConsoleColor.Black, ConsoleColor.Red);

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }

    private static void Write(object obj, ConsoleColor foreColor, ConsoleColor backColor)
    {
        ConsoleColor fg = Console.ForegroundColor;
        ConsoleColor bg = Console.BackgroundColor;
        Console.ForegroundColor = foreColor;
        Console.BackgroundColor = backColor;
        Console.Write(obj);
        Console.ForegroundColor = fg;
        Console.BackgroundColor = bg;
        Console.WriteLine();
    }
}