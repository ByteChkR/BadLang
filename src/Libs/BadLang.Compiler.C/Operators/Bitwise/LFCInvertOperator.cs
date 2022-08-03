using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Bitwise;

namespace LF.Compiler.C.Operators.Bitwise;

public class LFCInvertOperator : LFCPreValueOperator
{
    public LFCInvertOperator() : base(3, "~") { }

    public override LFCExpression Parse(LFCPreValueOperatorParseInput input)
    {
        return new LFCInvertExpression(input.Reader.CreatePosition(), input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}