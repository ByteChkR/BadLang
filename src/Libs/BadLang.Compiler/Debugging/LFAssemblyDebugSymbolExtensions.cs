using LF.Compiler.Reader;

namespace LF.Compiler.Debugging;

public static class LFAssemblyDebugSymbolExtensions
{
    public static LFAssemblyDebugSymbol CreateDebugSymbol(this LFSourcePosition position, string sectionName, int sectionOffset, string? symbolName = null)
    {
        return new LFAssemblyDebugSymbol(
            position.File,
            position.StartIndex,
            position.EndIndex,
            new LFAssemblyDebugSymbolReference(
                sectionName,
                sectionOffset,
                symbolName
            )
        );
    }
}