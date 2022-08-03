using LF.Compiler.Reader;

namespace LF.Compiler.C;

public class LFCParserException : Exception
{
    public LFCParserException(string message, LFSourcePosition position) : base($"{message} at {position}") { }
}