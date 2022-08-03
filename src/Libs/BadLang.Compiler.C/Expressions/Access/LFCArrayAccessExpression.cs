using LF.Compiler.C.Expressions.ControlFlow;
using LF.Compiler.C.Expressions.Pointers;
using LF.Compiler.C.Expressions.Value;
using LF.Compiler.C.Functions;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Access;

public class LFCArrayAccessExpression : LFCExpression
{
    public readonly LFCExpression Left;
    public readonly LFCExpression Right;

    public LFCArrayAccessExpression(LFSourcePosition position, LFCExpression left, LFCExpression right) : base(position)
    {
        Left = left;
        Right = right;
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCExpressionCompileResult leftResult = Left.Compile(input.CreateInput());

        if (leftResult.ReturnType is not LFCPointerType pointer)
        {
            throw new LFCParserException("Left side of array access must be a pointer", Left.Position);
        }

        LFCExpressionCompileResult rightResult = LFCCompilerFunctions.UnsafeCastNumber(input.CreateInput(isLValue: false), LFCBaseType.GetType("i64"), Right, Position);

        int elementSize = pointer.ElementType.GetSize();
        if (elementSize == 0)
        {
            throw new LFCParserException("Cannot access elements of a zero-sized type", Position);
        }

        if (elementSize != 1 || !LFCompilerOptimizationSettings.Instance.OptimizeOffsetCalculation)
        {
            input.Section.Emit(Position, OpCodes.Push64, (ulong)elementSize);
            input.Section.Emit(Position, OpCodes.Mul64);
        }
        
        

        input.Section.Emit(Position, OpCodes.Add64);

        return input.CreateResult(pointer, Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCArrayAccessExpression(Position, rebuilder(Left), rebuilder(Right));
    }
}