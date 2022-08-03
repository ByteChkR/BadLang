using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Math;

namespace LF.Compiler.C.Operators.Math;

public class LFCMultiplyOperator : LFCPostValueOperator
{
    public LFCMultiplyOperator() : base(5, "*") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCMultiplyExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}