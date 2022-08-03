namespace LF.Compiler.C.Expressions.Bitwise;

public class LFCAndExpression : LFCBitwiseExpression
{
    public LFCAndExpression(LFCExpression left, LFCExpression right) : base(left, right, "&", OpCodes.And8) { }

    public override string GetOperatorName()
    {
        return "op_And";
    }

    public override bool IsPositionDependent()
    {
        return false;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCAndExpression(rebuilder(Left), rebuilder(Right));
    }
}