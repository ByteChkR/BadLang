using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Math.Self;

namespace LF.Compiler.C.Operators.Math.Self;

public class LFCDivideSelfOperator : LFCPostValueOperator
{
    public LFCDivideSelfOperator() : base(16, "/=") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCDivideSelfExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}