using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.ControlFlow;

public class LFCContinueExpression :LFCExpression
{
    public LFCContinueExpression(LFSourcePosition position) : base(position) { }
    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        return input.Scope.Continue(input, Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCContinueExpression(Position);
    }
}

public class LFCBreakExpression :LFCExpression
{
    public LFCBreakExpression(LFSourcePosition position) : base(position) { }
    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        return input.Scope.Break(input, Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCBreakExpression(Position);
    }
}

public class LFCReturnExpression : LFCExpression
{
    public readonly LFCExpression? Expression;

    public LFCReturnExpression(LFSourcePosition start, LFCExpression? expression = null) : base(expression == null ? start : start.Combine(expression.Position))
    {
        Expression = expression;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCReturnExpression(Position, Expression == null ? null : rebuilder(Expression));
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        if (input.Scope.ReturnType != LFCBaseType.GetType("void") && Expression == null)
        {
            throw new LFCParserException($"Return value of type {input.Scope.ReturnType} expected", Position);
        }

        if (Expression != null)
        {
            LFCExpressionCompileResult result = Expression.Compile(
                new LFCExpressionCompileInput(
                    input.Result,
                    input.Section,
                    input.Scope,
                    false,
                    input.Scope.ReturnType
                )
            );
            if (!result.ReturnType.Equals(input.Scope.ReturnType))
            {
                throw new LFCParserException($"Return value of type {input.Scope.ReturnType} expected", Position);
            }
        }

        input.Scope.Return(input.Section, Position);

        input.Section.Emit(Position, OpCodes.Return);

        return input.CreateResult(input.Scope.ReturnType, Position, true); //Discarded. So the return value remains on the stack
    }
}