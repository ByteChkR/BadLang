namespace LF.Compiler.Debugging;

public readonly struct LFAssemblyDebugSymbolReference
{
    public readonly string SectionName;
    public readonly string? SymbolName;
    public readonly int SectionOffset;

    public LFAssemblyDebugSymbolReference(string sectionName, int sectionOffset, string? symbolName)
    {
        SectionName = sectionName;
        SectionOffset = sectionOffset;
        SymbolName = symbolName;
    }
}