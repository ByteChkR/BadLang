namespace LF.Compiler.C.Expressions;

public abstract class LFCBinaryExpression : LFCExpression
{
    public readonly LFCExpression Left;
    public readonly LFCExpression Right;

    protected LFCBinaryExpression(LFCExpression left, LFCExpression right) : base(left.Position.Combine(right.Position))
    {
        Left = left;
        Right = right;
    }
}