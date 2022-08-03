using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions;

public abstract class LFCExpression
{
    public readonly LFSourcePosition Position;

    protected LFCExpression(LFSourcePosition position)
    {
        Position = position;
    }

    public abstract LFCExpressionCompileResult Compile(LFCExpressionCompileInput input);
    public abstract LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder);
}