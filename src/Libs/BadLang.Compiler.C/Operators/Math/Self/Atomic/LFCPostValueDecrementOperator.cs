using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Math.Self.Atomic;

namespace LF.Compiler.C.Operators.Math.Self.Atomic;

public class LFCPostValueDecrementOperator : LFCPostValueOperator
{
    public LFCPostValueDecrementOperator() : base(2, "--") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCPostValueDecrementExpression(input.Left.Position.Combine(input.Reader.CreatePosition()), input.Left);
    }
}