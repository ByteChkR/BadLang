using System.Text;

using LF.Compiler.C.Expressions;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C;

public class LFCScope
{
    private readonly List<LFCScopeVariable> m_Parameters = new List<LFCScopeVariable>();
    private readonly Dictionary<LFCTypeToken, LFCTypeToken>? m_TypeArgs;
    private readonly List<LFCScopeVariable> m_Variables = new List<LFCScopeVariable>();
    
    private LFCChildScope? m_ChildScope;

    protected Func<LFCExpressionCompileInput, LFSourcePosition, LFCExpressionCompileResult>? OnBreak;
    protected  Func<LFCExpressionCompileInput, LFSourcePosition, LFCExpressionCompileResult>? OnContinue;

    public virtual LFCExpressionCompileResult Break(LFCExpressionCompileInput input, LFSourcePosition position)
    {
        if (OnBreak != null)
        {
            return OnBreak.Invoke(input, position);
        }

        throw new LFCParserException("Can not use break here", position);
    }

    public virtual LFCExpressionCompileResult Continue(LFCExpressionCompileInput input, LFSourcePosition position)
    {
        if (OnContinue != null)
        {
            return OnContinue.Invoke(input, position);
        }

        throw new LFCParserException("Can not use continue here", position);
    }

    public LFCScope(LFCType returnType, IEnumerable<KeyValuePair<LFCTypeToken, LFCTypeToken>> typeArgs)
    {
        ReturnType = returnType;
        m_TypeArgs = new Dictionary<LFCTypeToken, LFCTypeToken>(typeArgs);
    }

    public LFCScope(LFCTypeToken returnType, IEnumerable<KeyValuePair<LFCTypeToken, LFCTypeToken>> typeArgs, LFCTypeDatabase db, LFSourcePosition position)
    {
        m_TypeArgs = new Dictionary<LFCTypeToken, LFCTypeToken>(typeArgs);
        ReturnType = GetType(returnType, db, position);
    }

    public LFCType ReturnType { get; }

    public int TotalSize
    {
        get
        {
            int size = 0;
            foreach (LFCScopeVariable var in m_Variables)
            {
                size += var.Type.GetSize();
            }

            foreach (LFCScopeVariable lfcScopeVariable in m_Parameters)
            {
                size += lfcScopeVariable.Type.GetSize();
            }

            return size;
        }
    }

    public IEnumerable<KeyValuePair<LFCTypeToken, LFCTypeToken>> TypeArguments =>
        m_TypeArgs ?? Enumerable.Empty<KeyValuePair<LFCTypeToken, LFCTypeToken>>();

    public int VariableCount => m_Parameters.Count + m_Variables.Count;

    private int GetRelativeStart()
    {
        int off = 0;
        for (int i = m_Parameters.Count - 1; i >= 0; i--)
        {
            off -= m_Parameters[i].Type.GetSize();
        }

        return off;
    }

    public static Dictionary<LFCTypeToken, LFCTypeToken> CreateTypeMap(LFCType[] types, LFCTypeToken[] names)
    {
        return CreateTypeMap(types.Select(x => x.Name).ToArray(), names);
    }

    private static Dictionary<LFCTypeToken, LFCTypeToken> CreateTypeMap(LFCTypeToken[] types, LFCTypeToken[] names)
    {
        Dictionary<LFCTypeToken, LFCTypeToken> map = new Dictionary<LFCTypeToken, LFCTypeToken>();
        for (int i = 0; i < types.Length; i++)
        {
            map.Add(names[i], types[i]);
        }

        return map;
    }

    public static LFCTypeToken ResolveType(LFCTypeToken type, Dictionary<LFCTypeToken, LFCTypeToken>? typeArgs)
    {
        LFCTypeToken cleanType = new LFCTypeToken(LFCTypeDatabase.Parse(type, out int ptrLevel), type.TypeArgs);
        if (typeArgs != null && typeArgs.ContainsKey(cleanType))
        {
            cleanType = typeArgs[cleanType];
            StringBuilder sb = new StringBuilder(cleanType.Name);
            sb.Append('*', ptrLevel);
            type = new LFCTypeToken(sb.ToString(), cleanType.TypeArgs);
        }


        LFCTypeToken[] typeArguments = new LFCTypeToken[type.TypeArgs.Length];
        for (int i = 0; i < type.TypeArgs.Length; i++)
        {
            typeArguments[i] = ResolveType(type.TypeArgs[i], typeArgs);
        }

        return new LFCTypeToken(type.Name, typeArguments);
    }

    public static LFCType GetType(LFCTypeToken type, LFCTypeDatabase db, Dictionary<LFCTypeToken, LFCTypeToken>? typeArgs, LFSourcePosition position)
    {
        LFCTypeToken resolved = ResolveType(type, typeArgs);

        return db.GetType(resolved, position);
    }

    public LFCType GetType(LFCTypeToken type, LFCTypeDatabase db, LFSourcePosition position)
    {
        return GetType(type, db, m_TypeArgs, position);
    }

    public void PopVariables(LFReaderCodeSection section, int count)
    {
        for (int i = 0; i < count; i++)
        {
            LFCScopeVariable variable;
            if (m_Variables.Count != 0)
            {
                variable = m_Variables[^1];
                m_Variables.RemoveAt(m_Variables.Count - 1);
            }
            else
            {
                variable = m_Parameters[^1];
                m_Parameters.RemoveAt(m_Parameters.Count - 1);
            }

            section.Pop(variable.Type.GetSize(), LFSourcePosition.Unknown);
        }
    }

    public virtual void DefineParameter(string name, LFCType type, LFSourcePosition position)
    {
        m_Parameters.Add(new LFCScopeVariable(name, type));
    }

    public void ReleaseChild()
    {
        m_ChildScope = null;
    }

    public LFCChildScope CreateChildScope(
        LFSourcePosition position,
        Func<LFCExpressionCompileInput, LFSourcePosition, LFCExpressionCompileResult>? continueAction = null,
        Func<LFCExpressionCompileInput, LFSourcePosition, LFCExpressionCompileResult>? breakAction = null)
    {
        if (m_ChildScope != null)
        {
            throw new LFCParserException("Child scope already exists", position);
        }

        m_ChildScope = new LFCChildScope(this);

        m_ChildScope.OnBreak = breakAction;
        m_ChildScope.OnContinue = continueAction;
        return m_ChildScope;
    }

    public virtual void DefineOnStack(string name, LFCType type, LFReaderCodeSection section, LFSourcePosition position)
    {
        m_Variables.Add(new LFCScopeVariable(name, type));
        section.Push(type.GetSize(), position);
    }

    public void Return(LFReaderCodeSection section, LFSourcePosition position)
    {
        int retSize = ReturnType.GetSize();
        if (retSize != 0)
        {
            section.Emit(position, OpCodes.PushSF);
            section.Emit(position, OpCodes.Push64, (ulong)GetRelativeStart());
            section.Emit(position, OpCodes.Add64);
            section.Store(retSize, position);
        }

        int totalSize = TotalSize - ReturnType.GetSize();
        if (totalSize >= 0)
        {
            section.Pop(totalSize, position);
        }
        else
        {
            section.Emit(position, OpCodes.MoveSP, (ulong)-totalSize);
        }
    }

    public virtual bool HasVariable(string name)
    {
        return m_Variables.Any(x => x.Name == name) || m_Parameters.Any(x => x.Name == name);
    }

    public virtual LFCType GetVariableType(string name, LFSourcePosition position)
    {
        foreach (LFCScopeVariable v in m_Variables)
        {
            if (v.Name == name)
            {
                return v.Type;
            }
        }

        foreach (LFCScopeVariable v in m_Parameters)
        {
            if (v.Name == name)
            {
                return v.Type;
            }
        }

        throw new LFCParserException("Variable not found", position);
    }

    public virtual int GetOffset(string name, LFSourcePosition position)
    {
        int off = 0;
        foreach (LFCScopeVariable v in m_Variables)
        {
            if (v.Name == name)
            {
                return off;
            }

            off += v.Type.GetSize();
        }

        off = 0;
        for (int i = m_Parameters.Count - 1; i >= 0; i--)
        {
            LFCScopeVariable parameter = m_Parameters[i];
            off -= parameter.Type.GetSize();

            if (parameter.Name == name)
            {
                return off;
            }
        }

        throw new LFCParserException("Variable not found", position);
    }

    public void LoadAddress(LFReaderCodeSection section, string name, LFSourcePosition position)
    {
        section.Emit(position, OpCodes.PushSF);
        ulong off = (ulong)GetOffset(name, position);
        if (!LFCompilerOptimizationSettings.Instance.OptimizeOffsetCalculation || off != 0)
        {
            section.Emit(position, OpCodes.Push64, (ulong)GetOffset(name, position));
            section.Emit(position, OpCodes.Add64);
        }
    }

    public void LoadValue(LFReaderCodeSection section, string name, LFSourcePosition position)
    {
        section.Emit(position, OpCodes.PushSF);
        ulong off = (ulong)GetOffset(name, position);
        if (!LFCompilerOptimizationSettings.Instance.OptimizeOffsetCalculation || off != 0)
        {
            section.Emit(position, OpCodes.Push64, (ulong)GetOffset(name, position));
            section.Emit(position, OpCodes.Add64);
        }

        LFCType type = GetVariableType(name, position);
        section.Load(type.GetSize(), position);
    }

    private record struct LFCScopeVariable(string Name, LFCType Type);
}