namespace LF.Compiler.C.Expressions.Comparison;

public class LFCGreaterOrEqualExpression : LFCComparisonExpression
{
    public LFCGreaterOrEqualExpression(LFCExpression left, LFCExpression right) : base(
        left,
        right,
        ">=",
        OpCodes.TestGE8
    ) { }


    public override bool IsPositionDependent()
    {
        return true;
    }

    public override string GetOperatorName()
    {
        return "op_GreaterOrEqual";
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCGreaterOrEqualExpression(rebuilder(Left), rebuilder(Right));
    }
}