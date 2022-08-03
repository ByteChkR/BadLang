using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Math.Self.Atomic;

namespace LF.Compiler.C.Operators.Math.Self.Atomic;

public class LFCPostValueIncrementOperator : LFCPostValueOperator
{
    public LFCPostValueIncrementOperator() : base(2, "++") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCPostValueIncrementExpression(input.Left.Position.Combine(input.Reader.CreatePosition()), input.Left);
    }
}