using LF.Compiler.Reader;

namespace LF.Compiler.C.Operators;

public class LFCPreValueOperatorParseInput
{
    public LFReaderCodeSection CodeSection;
    public LFCReader Reader;
    public LFReaderResult Result;

    public LFCPreValueOperatorParseInput(LFReaderCodeSection codeSection, LFCReader reader, LFReaderResult result)
    {
        CodeSection = codeSection;
        Reader = reader;
        Result = result;
    }
}