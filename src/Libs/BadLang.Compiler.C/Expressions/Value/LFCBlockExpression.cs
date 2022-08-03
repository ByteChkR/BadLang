using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Value;

public class LFCBlockExpression : LFCExpression
{
    private readonly LFCExpression[] m_Block;

    public LFCBlockExpression(LFCExpression[] block) : base(CreatePosition(block.FirstOrDefault()?.Position, block.LastOrDefault()?.Position))
    {
        m_Block = block;
    }

    private static LFSourcePosition CreatePosition(LFSourcePosition? start, LFSourcePosition? end)
    {
        if (start == null)
        {
            return LFSourcePosition.Unknown;
        }

        if (end == null)
        {
            return start.Value;
        }

        return start.Value.Combine(end.Value);
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        foreach (LFCExpression expression in m_Block)
        {
            expression.Compile(input).DiscardResult();
        }

        return input.CreateResult(LFCBaseType.GetType("void"), Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        LFCExpression[] block = new LFCExpression[m_Block.Length];
        for (int i = 0; i < m_Block.Length; i++)
        {
            block[i] = rebuilder(m_Block[i]);
        }

        return new LFCBlockExpression(block);
    }
}