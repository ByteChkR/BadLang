namespace LF.Compiler.C.Expressions.Math;

public class LFCDivideExpression : LFCMathExpression
{
    public LFCDivideExpression(LFCExpression left, LFCExpression right) : base(left, right, "/", OpCodes.Div8) { }

    public override bool IsPositionDependent()
    {
        return true;
    }

    public override string GetOperatorName()
    {
        return "op_Div";
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCDivideExpression(rebuilder(Left), rebuilder(Right));
    }
}