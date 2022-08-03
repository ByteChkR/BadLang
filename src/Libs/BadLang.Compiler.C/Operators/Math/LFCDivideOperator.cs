using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Math;

namespace LF.Compiler.C.Operators.Math;

public class LFCDivideOperator : LFCPostValueOperator
{
    public LFCDivideOperator() : base(5, "/") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCDivideExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}