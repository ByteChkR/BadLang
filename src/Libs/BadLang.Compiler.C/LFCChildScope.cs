using LF.Compiler.C.Expressions;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C;

public class LFCChildScope : LFCScope
{
    private readonly LFCScope m_Parent;
    private readonly int m_VariableCount;

    public LFCChildScope(LFCScope parent) : base(parent.ReturnType, parent.TypeArguments)
    {
        m_Parent = parent;
        m_VariableCount = m_Parent.VariableCount;
    }

    public override LFCExpressionCompileResult Continue(LFCExpressionCompileInput input, LFSourcePosition position)
    {
        if (OnContinue != null)
        {
            return OnContinue.Invoke(input,position);
        }

        return m_Parent.Continue(input, position);
    }

    public override LFCExpressionCompileResult Break(LFCExpressionCompileInput input, LFSourcePosition position)
    {
        if (OnBreak != null)
        {
            return OnBreak.Invoke(input, position);
        }

        return m_Parent.Break(input, position);
    }

    public override void DefineParameter(string name, LFCType type, LFSourcePosition position)
    {
        throw new LFCParserException("Cannot define parameter in child scope", position);
    }

    public void Release(LFReaderCodeSection section)
    {
        m_Parent.PopVariables(section, m_Parent.VariableCount - m_VariableCount);
        m_Parent.ReleaseChild();
    }

    public override bool HasVariable(string name)
    {
        return m_Parent.HasVariable(name);
    }

    public override int GetOffset(string name, LFSourcePosition position)
    {
        return m_Parent.GetOffset(name, position);
    }

    public override LFCType GetVariableType(string name, LFSourcePosition position)
    {
        return m_Parent.GetVariableType(name, position);
    }

    public override void DefineOnStack(string name, LFCType type, LFReaderCodeSection section, LFSourcePosition position)
    {
        m_Parent.DefineOnStack(name, type, section, position);
    }
}