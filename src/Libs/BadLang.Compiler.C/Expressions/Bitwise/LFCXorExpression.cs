namespace LF.Compiler.C.Expressions.Bitwise;

public class LFCXorExpression : LFCBitwiseExpression
{
    public LFCXorExpression(LFCExpression left, LFCExpression right) : base(left, right, "^", OpCodes.XOr8) { }

    public override string GetOperatorName()
    {
        return "op_XOr";
    }

    public override bool IsPositionDependent()
    {
        return false;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCXorExpression(rebuilder(Left), rebuilder(Right));
    }
}