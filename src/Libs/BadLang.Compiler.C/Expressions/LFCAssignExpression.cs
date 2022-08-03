using LF.Compiler.C.Functions;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions;

public class LFCAssignExpression : LFCBinaryExpression
{
    public LFCAssignExpression(LFCExpression left, LFCExpression right) : base(left, right) { }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCAssignExpression(rebuilder(Left), rebuilder(Right));
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCExpressionCompileResult leftResult =
            Left.Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, true));
        if (leftResult.ReturnType is not LFCPointerType pointer)
        {
            throw new LFCParserException("Left side of assignment must be a pointer", Position);
        }


        LFCExpressionCompileResult rightResult = Right.Compile(
            new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false, pointer.ElementType)
        );

        if (!rightResult.EmittedAny)
        {
            if (rightResult.ReturnType is LFCFunctionType funcType)
            {
                if (funcType.SourceFunction == null)
                {
                    throw new LFCParserException("Function type must have a source function", Position);
                }

                input.Section.Emit(Position, OpCodes.Push64, funcType.SourceFunction.GetFullName(input.Section));
            }
            else
            {
                throw new LFCParserException("Right side of assignment must be a value", Position);
            }
        }

        if (pointer.ElementType != rightResult.ReturnType)
        {
            throw new LFCParserException("Left side of assignment must be a pointer to the same type as the right side", Position);
        }


        input.Section.Assign(pointer.ElementType.GetSize(), Position);


        return input.CreateResult(pointer.ElementType, Position, true);
    }
}