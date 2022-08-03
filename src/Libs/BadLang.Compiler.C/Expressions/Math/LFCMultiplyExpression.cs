namespace LF.Compiler.C.Expressions.Math;

public class LFCMultiplyExpression : LFCMathExpression
{
    public LFCMultiplyExpression(LFCExpression left, LFCExpression right) : base(left, right, "*", OpCodes.Mul8) { }

    public override bool IsPositionDependent()
    {
        return false;
    }

    public override string GetOperatorName()
    {
        return "op_Mul";
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCMultiplyExpression(rebuilder(Left), rebuilder(Right));
    }
}