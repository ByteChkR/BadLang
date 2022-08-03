namespace LF.Compiler.C.Expressions.Math;

public class LFCSubtractExpression : LFCMathExpression
{
    public LFCSubtractExpression(LFCExpression left, LFCExpression right) : base(left, right, "-", OpCodes.Sub8) { }

    public override bool IsPositionDependent()
    {
        return true;
    }

    public override string GetOperatorName()
    {
        return "op_Sub";
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCSubtractExpression(rebuilder(Left), rebuilder(Right));
    }
}