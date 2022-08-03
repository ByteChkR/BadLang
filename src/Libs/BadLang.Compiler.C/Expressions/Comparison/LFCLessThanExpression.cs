namespace LF.Compiler.C.Expressions.Comparison;

public class LFCLessThanExpression : LFCComparisonExpression
{
    public LFCLessThanExpression(LFCExpression left, LFCExpression right) : base(left, right, "<", OpCodes.TestLT8) { }

    public override bool IsPositionDependent()
    {
        return true;
    }

    public override string GetOperatorName()
    {
        return "op_LessThan";
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCLessThanExpression(rebuilder(Left), rebuilder(Right));
    }
}