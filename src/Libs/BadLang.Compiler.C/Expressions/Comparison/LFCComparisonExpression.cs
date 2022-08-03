using LF.Compiler.C.Expressions.ControlFlow;
using LF.Compiler.C.Expressions.Value;
using LF.Compiler.C.Functions;
using LF.Compiler.C.Types;

namespace LF.Compiler.C.Expressions.Comparison;

public abstract class LFCComparisonExpression : LFCExpression
{
    public readonly LFCExpression Left;
    private readonly OpCodes m_OpCode;
    private readonly string m_Symbol;
    public readonly LFCExpression Right;

    protected LFCComparisonExpression(LFCExpression left, LFCExpression right, string symbol, OpCodes op8) : base(left.Position.Combine(right.Position))
    {
        Left = left;
        Right = right;
        m_Symbol = symbol;
        m_OpCode = op8;
    }

    public abstract bool IsPositionDependent();
    public abstract string GetOperatorName();

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCExpressionCompileResult leftResult =
            Left.Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false));


        if (!LFCBaseType.IsBaseType(leftResult.ReturnType))
        {
            LFCExpressionCompileResult rightOpResult =
                Right.Compile(
                    new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false)
                );
            LFCTypeOverrideSelector selector = new LFCTypeOverrideSelector(leftResult.ReturnType, rightOpResult.ReturnType, IsPositionDependent(), GetOperatorName());
            leftResult.RemoveEmitted();
            LFCFunctionType op = selector.GetOverride(Position, out bool isReverseOrder);
            LFCExpression[] parameters;
            if (isReverseOrder)
            {
                parameters = new[] { Right, Left };
            }
            else
            {
                parameters = new[] { Left, Right };
            }

            LFCInvocationExpression invoc = new LFCInvocationExpression(
                Position,
                new LFCVariableExpression(op.SourceFunction!.GetFullName(input.Section), Position),
                parameters,
                Array.Empty<LFCTypeToken>()
            );

            return invoc.Compile(input.CreateInput(isLValue: false, typeHint: input.TypeHint));

            throw new LFCParserException($"Left side of {m_Symbol} operator must be a base type", Position);
        }

        LFCExpressionCompileResult rightResult =
            Right.Compile(
                new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false)
            );
        if (!LFCBaseType.IsBaseType(rightResult.ReturnType))
        {
            LFCTypeOverrideSelector selector = new LFCTypeOverrideSelector(leftResult.ReturnType, rightResult.ReturnType, IsPositionDependent(), GetOperatorName());
            leftResult.RemoveEmitted();
            LFCFunctionType op = selector.GetOverride(Position, out bool isReverseOrder);
            LFCExpression[] parameters;
            if (isReverseOrder)
            {
                parameters = new[] { Right, Left };
            }
            else
            {
                parameters = new[] { Left, Right };
            }

            LFCInvocationExpression invoc = new LFCInvocationExpression(
                Position,
                new LFCVariableExpression(op.SourceFunction!.GetFullName(input.Section), Position),
                parameters,
                Array.Empty<LFCTypeToken>()
            );

            return invoc.Compile(input.CreateInput(isLValue: false, typeHint: input.TypeHint));

            throw new LFCParserException($"Right side of {m_Symbol} operator must be a base type", Position);
        }

        leftResult.RemoveEmitted();
        leftResult =
            Left.Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false, input.TypeHint));

        rightResult =
            Right.Compile(
                new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false, leftResult.ReturnType)
            );

        if (leftResult.ReturnType != rightResult.ReturnType)
        {
            throw new LFCParserException("Operands must be of the same type", Position);
        }


        switch (leftResult.ReturnType.GetSize())
        {
            case 1:
                input.Section.Emit(Position, m_OpCode);

                break;
            case 2:
                input.Section.Emit(Position, m_OpCode + 1);

                break;
            case 4:
                input.Section.Emit(Position, m_OpCode + 2);

                break;
            case 8:
                input.Section.Emit(Position, m_OpCode + 3);

                break;
            default:
                throw new LFCParserException($"Unsupported size {leftResult.ReturnType.GetSize()}", Position);
        }

        return input.CreateResult(leftResult.ReturnType, Position);
    }
}