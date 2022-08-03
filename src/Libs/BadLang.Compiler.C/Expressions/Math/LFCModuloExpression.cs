namespace LF.Compiler.C.Expressions.Math;

public class LFCModuloExpression : LFCMathExpression
{
    public LFCModuloExpression(LFCExpression left, LFCExpression right) : base(left, right, "%", OpCodes.Mod8) { }

    public override bool IsPositionDependent()
    {
        return true;
    }

    public override string GetOperatorName()
    {
        return "op_Mod";
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCModuloExpression(rebuilder(Left), rebuilder(Right));
    }
}