namespace LF.Compiler.C.Expressions.Comparison;

public class LFCGreaterThanExpression : LFCComparisonExpression
{
    public LFCGreaterThanExpression(LFCExpression left, LFCExpression right) :
        base(left, right, ">", OpCodes.TestGT8) { }

    public override bool IsPositionDependent()
    {
        return true;
    }

    public override string GetOperatorName()
    {
        return "op_GreaterThan";
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCGreaterThanExpression(rebuilder(Left), rebuilder(Right));
    }
}