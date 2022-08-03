using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Math;

namespace LF.Compiler.C.Operators.Math;

public class LFCSubtractOperator : LFCPostValueOperator
{
    public LFCSubtractOperator() : base(6, "-") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCSubtractExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}