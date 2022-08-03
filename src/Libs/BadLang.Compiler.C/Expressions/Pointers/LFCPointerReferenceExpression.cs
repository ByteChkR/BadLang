using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Pointers;

public class LFCPointerReferenceExpression : LFCExpression
{
    public readonly LFCExpression Expression;

    public LFCPointerReferenceExpression(LFSourcePosition start, LFCExpression expression) : base(start.Combine(expression.Position))
    {
        Expression = expression;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCPointerReferenceExpression(Position, rebuilder(Expression));
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        return Expression.Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, true));
    }
}