using LF.Compiler.Reader;

namespace LF.Compiler.Assembly;

public class LFAssemblyParser : LFCodeParser
{
    public static void AddParser()
    {
        AddParser<LFAssemblyParser>(".lfasm");
    }

    protected override LFCodeReader CreateReader(string src, string workingDir, string fileName)
    {
        return new LFAssemblyReader(src, workingDir, fileName);
    }
}