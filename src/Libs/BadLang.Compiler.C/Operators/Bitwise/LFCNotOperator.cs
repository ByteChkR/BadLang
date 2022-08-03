using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Bitwise;

namespace LF.Compiler.C.Operators.Bitwise;

public class LFCNotOperator : LFCPreValueOperator
{
    public LFCNotOperator() : base(3, "!") { }

    public override LFCExpression Parse(LFCPreValueOperatorParseInput input)
    {
        return new LFCNotExpression(input.Reader.CreatePosition(), input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}