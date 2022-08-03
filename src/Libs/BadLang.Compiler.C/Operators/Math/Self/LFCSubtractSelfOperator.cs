using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Math.Self;

namespace LF.Compiler.C.Operators.Math.Self;

public class LFCSubtractSelfOperator : LFCPostValueOperator
{
    public LFCSubtractSelfOperator() : base(16, "-=") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCSubtractSelfExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}