using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions;

public class LFCExpressionCompileResult
{
    public readonly LFCTypeToken[] InheritedTypeArgs;
    private readonly LFReaderResult m_Result;
    private readonly LFReaderCodeSection m_Section;
    private readonly int m_StartSectionIndex;

    public readonly LFCType ReturnType;
    private bool m_Discarded;

    public LFCExpressionCompileResult(
        LFCType returnType,
        LFReaderResult result,
        LFReaderCodeSection section,
        int startSectionIndex,
        bool discarded = false,
        LFCTypeToken[]? inheritedTypeArgs = null)
    {
        ReturnType = returnType;
        m_Result = result;
        m_Section = section;
        m_Discarded = discarded;
        EmittedAny = startSectionIndex != section.InstructionCount;
        m_StartSectionIndex = startSectionIndex;
        InheritedTypeArgs = inheritedTypeArgs ?? Array.Empty<LFCTypeToken>();
    }

    public bool EmittedAny { get; }

    public void RemoveEmitted()
    {
        m_Section.RemoveInstructions(m_Section.InstructionCount - m_StartSectionIndex);
    }


    public void DiscardResult()
    {
        if (m_Discarded)
        {
            return;
        }

        m_Discarded = true;
        m_Section.Pop(ReturnType.GetSize(), LFSourcePosition.Unknown);
    }
}