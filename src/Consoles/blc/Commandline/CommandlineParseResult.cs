namespace LFC.Commandline;

public class CommandlineParseResult<T>
{
    public readonly T Object;
    public readonly string[] RemainingArguments;

    public CommandlineParseResult(T o, string[] remainingArguments)
    {
        Object = o;
        RemainingArguments = remainingArguments;
    }
}