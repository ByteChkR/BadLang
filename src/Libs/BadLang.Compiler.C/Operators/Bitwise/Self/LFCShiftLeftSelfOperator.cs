using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Bitwise.Self;

namespace LF.Compiler.C.Operators.Bitwise.Self;

public class LFCShiftLeftSelfOperator : LFCPostValueOperator
{
    public LFCShiftLeftSelfOperator() : base(16, "<<=") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCShiftLeftSelfExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}