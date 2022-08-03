using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Bitwise;

public class LFCNotExpression : LFCExpression
{
    public readonly LFCExpression Expression;

    public LFCNotExpression(LFSourcePosition start, LFCExpression expression) : base(start.Combine(expression.Position))
    {
        Expression = expression;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCNotExpression(Position, rebuilder(Expression));
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCExpressionCompileResult result = Expression.Compile(
            new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false)
        );
        switch (result.ReturnType.GetSize())
        {
            case 1:
                input.Section.Emit(Position, OpCodes.TestZero8);

                break;
            case 2:
                input.Section.Emit(Position, OpCodes.TestZero16);

                break;
            case 4:
                input.Section.Emit(Position, OpCodes.TestZero32);

                break;
            case 8:
                input.Section.Emit(Position, OpCodes.TestZero64);

                break;
            default:
                throw new LFCParserException($"Invalid size {result.ReturnType.GetSize()}", Position);
        }

        return input.CreateResult(result.ReturnType, Position);
    }
}