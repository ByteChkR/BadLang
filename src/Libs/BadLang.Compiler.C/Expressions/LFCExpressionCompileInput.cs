using LF.Compiler.C.Functions;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions;

public class LFCExpressionCompileInput
{
    private readonly int m_StartSectionIndex;
    public bool IsLValue;
    public LFReaderResult Result;
    public LFCScope Scope;
    public LFReaderCodeSection Section;
    public LFCType? TypeHint;

    public LFCExpressionCompileInput(
        LFReaderResult result,
        LFReaderCodeSection section,
        LFCScope scope,
        bool isLValue,
        LFCType? hint = null)
    {
        Result = result;
        Section = section;
        Scope = scope;
        IsLValue = isLValue;
        TypeHint = hint;
        m_StartSectionIndex = Section.InstructionCount;
    }

    public LFCType GetType(string name, LFSourcePosition position)
    {
        return GetType(new LFCTypeToken(name, Array.Empty<LFCTypeToken>()), position);
    }

    public LFCType GetType(LFCTypeToken name, LFSourcePosition position)
    {
        LFCTypeDatabase db = LFCTypeDatabase.GetTypeSystem(Result);

        LFCType type = Scope.GetType(name, db, position);
        if (type is LFCStructureType structType)
        {
            if (structType.ContainingSection != Section && !Section.HasImport(structType.ContainingSection.Name))
            {
                throw new LFCParserException("Cannot access structure type from outside of its section", position);
            }
        }

        return type;
    }

    public bool HasFunction(string name)
    {
        return LFCFunctionTable.GetFunctionTable(Section, Result).HasFunction(name);
    }

    public LFCType LoadFunction(string name, LFSourcePosition position)
    {
        LFCFunction func = LFCFunctionTable.GetFunctionTable(Section, Result).GetFunction(name, position);

        return LFCFunctionType.GetType(func);
    }

    public LFCExpressionCompileResult CreateResult(LFCType returnType, LFSourcePosition position, bool discarded = false)
    {
        if (TypeHint != null && returnType != TypeHint)
        {
            throw new LFCParserException("Type hint does not match return type", position);
        }

        return new LFCExpressionCompileResult(returnType, Result, Section, m_StartSectionIndex, discarded);
    }

    public LFCExpressionCompileInput CreateInput(
        LFReaderResult? result = null,
        LFReaderCodeSection? section = null,
        LFCScope? scope = null,
        LFCType? typeHint = null,
        bool? isLValue = null)
    {
        return new LFCExpressionCompileInput(result ?? Result, section ?? Section, scope ?? Scope, isLValue ?? IsLValue, typeHint);
    }

    public LFCExpressionCompileResult CreateResult(LFCType returnType, LFCTypeToken[] inheritedTypeArgs, LFSourcePosition position)
    {
        if (TypeHint != null && returnType != TypeHint)
        {
            throw new LFCParserException("Type hint does not match return type", position);
        }

        return new LFCExpressionCompileResult(returnType, Result, Section, m_StartSectionIndex, false, inheritedTypeArgs);
    }
}