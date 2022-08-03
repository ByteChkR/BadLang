using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Bitwise;

namespace LF.Compiler.C.Operators.Bitwise;

public class LFCShiftRightOperator : LFCPostValueOperator
{
    public LFCShiftRightOperator() : base(7, ">>") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCShiftRightExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}