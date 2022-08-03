namespace LF.Compiler.C.Expressions.Bitwise;

public class LFCOrExpression : LFCBitwiseExpression
{
    public LFCOrExpression(LFCExpression left, LFCExpression right) : base(left, right, "|", OpCodes.Or8) { }

    public override string GetOperatorName()
    {
        return "op_Or";
    }

    public override bool IsPositionDependent()
    {
        return false;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCOrExpression(rebuilder(Left), rebuilder(Right));
    }
}