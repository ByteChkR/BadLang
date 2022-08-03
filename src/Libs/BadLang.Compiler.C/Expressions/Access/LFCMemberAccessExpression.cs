using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Access;

public class LFCMemberAccessExpression : LFCExpression
{
    public readonly bool IsPointerAccess;
    public readonly LFCExpression Left;
    public readonly string Right;

    public LFCMemberAccessExpression(LFSourcePosition position, LFCExpression left, string right, bool pointerAccess) : base(position)
    {
        Left = left;
        Right = right;
        IsPointerAccess = pointerAccess;
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCExpressionCompileResult leftResult = Left.Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, !IsPointerAccess));

        LFCType leftType = leftResult.ReturnType;
        if (leftType is not LFCPointerType leftPtr)
        {
            throw new LFCParserException("Pointer access to non-pointer type", Position);
        }

        leftType = leftPtr.ElementType;

        if (leftType.HasProperty(LFCStructureType.GetFunctionName(leftType.Name.Name, Right)))
        {
            string funcName = LFCStructureType.GetFunctionName(leftType.Name.Name, Right);

            LFCType funcType = leftType.GetPropertyType(funcName, Position);

            return input.CreateResult(funcType, leftType.Name.TypeArgs, Position);
        }

        if (!leftType.HasProperty(Right))
        {
            throw new LFCParserException($"Property {Right} not found in {leftResult.ReturnType.Name}", Position);
        }

        int offset = leftType.GetPropertyOffset(Right, Position);
        LFCType memberType = leftType.GetPropertyType(Right, Position);

        if (!LFCompilerOptimizationSettings.Instance.OptimizeOffsetCalculation || offset != 0)
        {
            input.Section.Emit(Position, OpCodes.Push64, (ulong)offset);
            input.Section.Emit(Position, OpCodes.Add64);
        }

        if (!input.IsLValue)
        {
            input.Section.Load(memberType.GetSize(), Position);

            return input.CreateResult(memberType, Position);
        }

        return input.CreateResult(LFCPointerType.Create(memberType), Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCMemberAccessExpression(Position, rebuilder(Left), Right, IsPointerAccess);
    }
}