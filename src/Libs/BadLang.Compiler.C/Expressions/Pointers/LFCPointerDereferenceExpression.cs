using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Pointers;

public class LFCPointerDereferenceExpression : LFCExpression
{
    public readonly LFCExpression Expression;

    public LFCPointerDereferenceExpression(LFSourcePosition start, LFCExpression expression) : base(start.Combine(expression.Position))
    {
        Expression = expression;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCPointerDereferenceExpression(Position, rebuilder(Expression));
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCExpressionCompileResult result = Expression.Compile(
            new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false)
        );
        if (result.ReturnType is not LFCPointerType pointer)
        {
            throw new LFCParserException("Cannot dereference a non-pointer type", Position);
        }

        if (!input.IsLValue)
        {
            input.Section.Load(pointer.ElementType.GetSize(), Position);

            return input.CreateResult(pointer.ElementType, Position);
        }

        return input.CreateResult(pointer, Position);
    }
}