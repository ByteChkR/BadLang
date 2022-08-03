using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Pointers;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Operators.Pointers;

public class LFCPointerReferenceOperator : LFCPreValueOperator
{
    public LFCPointerReferenceOperator() : base(3, "&") { }

    public override LFCExpression Parse(LFCPreValueOperatorParseInput input)
    {
        LFSourcePosition start = input.Reader.CreatePosition();
        LFCExpression left = input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence);

        return new LFCPointerReferenceExpression(start, left);
    }
}