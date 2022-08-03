using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Math;

namespace LF.Compiler.C.Operators.Math;

public class LFCAddOperator : LFCPostValueOperator
{
    public LFCAddOperator() : base(6, "+") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCAddExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}