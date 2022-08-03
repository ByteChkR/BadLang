namespace LF.Compiler.Debugging;

public readonly struct LFAssemblyDebugSymbol
{
    public readonly string FileName;
    public readonly int StartIndex;
    public readonly int EndIndex;
    public readonly LFAssemblyDebugSymbolReference Reference;

    public string Export()
    {
        return $"{Reference.SectionName}|{Reference.SymbolName ?? ""}|{Reference.SectionOffset}|{FileName}|{StartIndex}|{EndIndex}";
    }

    public static LFAssemblyDebugSymbol Parse(string line)
    {
        string[] parts = line.Split('|');

        return new LFAssemblyDebugSymbol(
            parts[3],
            int.Parse(parts[4]),
            int.Parse(parts[5]),
            new LFAssemblyDebugSymbolReference(
                parts[0],
                int.Parse(parts[2]),
                string.IsNullOrEmpty(parts[1]) ? null : parts[1]
            )
        );
    }

    public LFAssemblyDebugSymbol(string fileName, int startIndex, int endIndex, LFAssemblyDebugSymbolReference reference)
    {
        FileName = fileName;
        StartIndex = startIndex;
        EndIndex = endIndex;
        Reference = reference;
    }
}