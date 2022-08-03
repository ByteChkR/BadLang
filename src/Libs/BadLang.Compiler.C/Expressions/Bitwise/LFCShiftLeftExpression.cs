namespace LF.Compiler.C.Expressions.Bitwise;

public class LFCShiftLeftExpression : LFCBitwiseExpression
{
    public LFCShiftLeftExpression(LFCExpression left, LFCExpression right) : base(left, right, "<<", OpCodes.Shl8) { }

    public override string GetOperatorName()
    {
        return "op_ShiftLeft";
    }

    public override bool IsPositionDependent()
    {
        return false;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCShiftLeftExpression(rebuilder(Left), rebuilder(Right));
    }
}