using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Comparison;

namespace LF.Compiler.C.Operators.Comparison;

public class LFCNotEqualOperator : LFCPostValueOperator
{
    public LFCNotEqualOperator() : base(9, "!=") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCNotEqualExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}