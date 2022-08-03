namespace LF.Compiler.C.Expressions.Bitwise;

public class LFCShiftRightExpression : LFCBitwiseExpression
{
    public LFCShiftRightExpression(LFCExpression left, LFCExpression right) : base(left, right, ">>", OpCodes.Shr8) { }

    public override string GetOperatorName()
    {
        return "op_ShiftRight";
    }

    public override bool IsPositionDependent()
    {
        return false;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCShiftRightExpression(rebuilder(Left), rebuilder(Right));
    }
}