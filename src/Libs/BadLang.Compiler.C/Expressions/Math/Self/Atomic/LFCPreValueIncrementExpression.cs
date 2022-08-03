using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Math.Self.Atomic;

public class LFCPreValueIncrementExpression : LFCExpression
{
    private readonly LFCExpression Left;

    public LFCPreValueIncrementExpression(LFSourcePosition position, LFCExpression left) : base(position)
    {
        Left = left;
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCExpressionCompileResult left = Left.Compile(input.CreateInput(isLValue: false));
        ulong value = 1;
        if (left.ReturnType is LFCPointerType pointer)
        {
            value = (ulong)pointer.ElementType.GetSize();
            if (value == 0)
            {
                throw new LFCParserException("Cannot increment a pointer of size 0", Position);
            }
        }

        input.Section.Push(left.ReturnType.GetSize(), value, Position);
        input.Section.Add(left.ReturnType.GetSize(), Position);
        input.Section.Dup(left.ReturnType.GetSize(), Position);

        LFCExpressionCompileResult leftPtr = Left.Compile(input.CreateInput(isLValue: true));
        if (leftPtr.ReturnType is not LFCPointerType)
        {
            throw new LFCParserException("Can not write to a non-pointer", Position);
        }

        input.Section.Store(left.ReturnType.GetSize(), Position);

        return input.CreateResult(left.ReturnType, Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCPreValueIncrementExpression(Position, rebuilder(Left));
    }
}