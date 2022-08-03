using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Value;

public class LFCValueExpression : LFCExpression
{
    private readonly ulong m_Value;

    public LFCValueExpression(ulong value, LFSourcePosition position) : base(position)
    {
        m_Value = value;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return this;
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        if (input.IsLValue)
        {
            throw new LFCParserException("Cannot assign to a value", Position);
        }

        if (input.TypeHint == null)
        {
            input.Section.Emit(Position, OpCodes.Push64, m_Value);

            return input.CreateResult(LFCBaseType.GetType("i64"), Position);
        }

        if (!LFCBaseType.BaseTypes.Contains(input.TypeHint))
        {
            throw new LFCParserException("Invalid type hint", Position);
        }

        switch (input.TypeHint.GetSize())
        {
            case 1:
                input.Section.Emit(Position, OpCodes.Push8, (byte)m_Value);

                break;
            case 2:
                input.Section.Emit(Position, OpCodes.Push16, (ushort)m_Value);

                break;
            case 4:
                input.Section.Emit(Position, OpCodes.Push32, (uint)m_Value);

                break;
            case 8:
                input.Section.Emit(Position, OpCodes.Push64, m_Value);

                break;
            default:
                throw new LFCParserException($"Invalid type hint {input.TypeHint}", Position);
        }

        return input.CreateResult(input.TypeHint, Position);
    }
}