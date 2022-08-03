namespace LF.Compiler.C.Expressions.Comparison;

public class LFCEqualExpression : LFCComparisonExpression
{
    public LFCEqualExpression(LFCExpression left, LFCExpression right) : base(left, right, "==", OpCodes.TestEq8) { }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCEqualExpression(rebuilder(Left), rebuilder(Right));
    }


    public override bool IsPositionDependent()
    {
        return false;
    }

    public override string GetOperatorName()
    {
        return "op_Equal";
    }
}