namespace LF.Compiler.C.Expressions.Math;

public class LFCAddExpression : LFCMathExpression
{
    public LFCAddExpression(LFCExpression left, LFCExpression right) : base(left, right, "+", OpCodes.Add8) { }

    public override bool IsPositionDependent()
    {
        return false;
    }

    public override string GetOperatorName()
    {
        return "op_Add";
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCAddExpression(rebuilder(Left), rebuilder(Right));
    }
}