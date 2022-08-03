namespace LF.Compiler.C.Expressions.Comparison;

public class LFCNotEqualExpression : LFCComparisonExpression
{
    public LFCNotEqualExpression(LFCExpression left, LFCExpression right) :
        base(left, right, "!=", OpCodes.TestNEq8) { }


    public override bool IsPositionDependent()
    {
        return false;
    }

    public override string GetOperatorName()
    {
        return "op_NotEqual";
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCNotEqualExpression(rebuilder(Left), rebuilder(Right));
    }
}