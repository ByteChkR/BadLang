using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Bitwise.Self;

public class LFCOrSelfExpression : LFCOrExpression
{
    public LFCOrSelfExpression(LFCExpression left, LFCExpression right) : base(left, right) { }

    public override string GetOperatorName()
    {
        return "op_OrSelf";
    }

    public override bool IsPositionDependent()
    {
        return true;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCOrSelfExpression(rebuilder(Left), rebuilder(Right));
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCExpressionCompileResult leftResult = Left.Compile(input.CreateInput(isLValue: true));
        leftResult.RemoveEmitted();

        if (leftResult.ReturnType is not LFCPointerType pointer)
        {
            throw new LFCParserException("Left side of += assignment must be a pointer", Left.Position);
        }

        if (!LFCBaseType.IsBaseType(pointer.ElementType))
        {
            LFCExpressionCompileResult? ovResult = CompileOverride(input, true);

            if (ovResult != null)
            {
                return ovResult;
            }
        }


        leftResult = Left.Compile(input.CreateInput(isLValue: true));

        if (leftResult.ReturnType is not LFCPointerType ptr)
        {
            throw new LFCParserException("Left side of += assignment must be a pointer", Left.Position);
        }


        LFCExpressionCompileResult output = base.Compile(input.CreateInput(isLValue: false, typeHint: ptr.ElementType));


        if (output.ReturnType != ptr.ElementType)
        {
            throw new LFCParserException("Left side of += assignment must be a pointer to the same type as the right side", Left.Position);
        }

        input.Section.Assign(ptr.ElementType.GetSize(), Position);

        return input.CreateResult(ptr.ElementType, Position, true);
    }
}