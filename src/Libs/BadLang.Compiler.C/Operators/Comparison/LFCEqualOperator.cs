using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Comparison;

namespace LF.Compiler.C.Operators.Comparison;

public class LFCEqualOperator : LFCPostValueOperator
{
    public LFCEqualOperator() : base(9, "==") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCEqualExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}