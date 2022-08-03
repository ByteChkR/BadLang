using LF.Compiler.C.Expressions;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Operators;

public class LFCPostValueOperatorParseInput
{
    public LFReaderCodeSection CodeSection;
    public LFCExpression Left;
    public LFCReader Reader;
    public LFReaderResult Result;

    public LFCPostValueOperatorParseInput(LFCExpression left, LFReaderCodeSection codeSection, LFCReader reader, LFReaderResult result)
    {
        Left = left;
        CodeSection = codeSection;
        Reader = reader;
        Result = result;
    }
}