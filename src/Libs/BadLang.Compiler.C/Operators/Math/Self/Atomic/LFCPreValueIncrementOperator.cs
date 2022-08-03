using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Math.Self.Atomic;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Operators.Math.Self.Atomic;

public class LFCPreValueIncrementOperator : LFCPreValueOperator
{
    public LFCPreValueIncrementOperator() : base(3, "++") { }

    public override LFCExpression Parse(LFCPreValueOperatorParseInput input)
    {
        LFSourcePosition start = input.Reader.CreatePosition();
        LFCExpression left = input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence);

        return new LFCPreValueIncrementExpression(start, left);
    }
}