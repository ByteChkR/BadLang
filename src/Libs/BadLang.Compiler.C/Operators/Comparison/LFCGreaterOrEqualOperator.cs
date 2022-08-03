using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Comparison;

namespace LF.Compiler.C.Operators.Comparison;

public class LFCGreaterOrEqualOperator : LFCPostValueOperator
{
    public LFCGreaterOrEqualOperator() : base(8, ">=") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCGreaterOrEqualExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}