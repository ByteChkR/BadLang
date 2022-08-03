namespace LF.Compiler.C.Expressions.Comparison;

public class LFCLessOrEqualExpression : LFCComparisonExpression
{
    public LFCLessOrEqualExpression(LFCExpression left, LFCExpression right) :
        base(left, right, "<=", OpCodes.TestLE8) { }

    public override bool IsPositionDependent()
    {
        return true;
    }

    public override string GetOperatorName()
    {
        return "op_LessOrEqual";
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCLessOrEqualExpression(rebuilder(Left), rebuilder(Right));
    }
}