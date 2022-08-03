using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Functions;

public abstract class LFCFunction
{
    public readonly LFCTypeToken? InstanceTypeName;
    public readonly bool IsInstance;
    private readonly LFCFunctionParameter[] m_Parameters;
    private readonly LFCTypeToken[] m_TypeParameters;

    public readonly string Name;
    public readonly LFSourcePosition Position;
    public readonly LFCTypeToken ReturnTypeName;

    protected LFCFunction(
        string name,
        LFCTypeToken returnType,
        LFCFunctionParameter[] parameters,
        LFCTypeToken[] typeParameters,
        LFReaderCodeSection containingSection,
        bool isInstance,
        LFCTypeToken? instanceTypeName,
        LFSourcePosition position)
    {
        Name = name;
        ReturnTypeName = returnType;
        m_Parameters = parameters;
        m_TypeParameters = typeParameters;
        ContainingSection = containingSection;
        IsInstance = isInstance;
        InstanceTypeName = instanceTypeName;
        Position = position;
    }

    public bool AutoExport { get; set; }

    public LFReaderCodeSection ContainingSection { get; }

    public int TypeParameterCount => m_TypeParameters.Length;
    public int ParameterCount => m_Parameters.Length;
    public bool HasTypeParameters => m_TypeParameters.Length != 0 || IsInstance && InstanceTypeName != null && InstanceTypeName.Value.TypeArgs.Length != 0;
    public IEnumerable<LFCTypeToken> TypeParameters => m_TypeParameters;
    public IEnumerable<LFCFunctionParameter> Parameters => m_Parameters;

    public abstract LFCFunction CreateInstance(LFCTypeToken instanceType);
    public abstract LFCFunction CreateOperator(LFCTypeToken instanceType);

    public string GetFullName(LFReaderCodeSection section)
    {
        if (section == ContainingSection)
        {
            return Name;
        }

        return $"{ContainingSection.Name}::{Name}";
    }

    public static string GetFunctionLabel(string name, LFCType[] typeArgs)
    {
        return name +
               (typeArgs.Length == 0 ? "" : "<") +
               string.Join(", ", typeArgs.Select(t => t.Name)) +
               (typeArgs.Length == 0 ? "" : ">");
    }

    public LFCTypeToken GetTypeParameter(int i)
    {
        return m_TypeParameters[i];
    }

    public LFCTypeToken GetParameter(int i)
    {
        return m_Parameters[i].TypeName;
    }

    public abstract void Compile(LFReaderResult result);
    public abstract void Compile(LFReaderResult result, LFCType[] typeParameters);

    public virtual string GetPrintableName()
    {
        return $"{ContainingSection.Name}::{Name}";
    }

    public override string ToString()
    {
        return
            $"{ReturnTypeName} {GetPrintableName()}{(m_TypeParameters.Length == 0 ? "" : $"<{string.Join(", ", m_TypeParameters.Select(x => x.ToString()))}>")}({string.Join(", ", m_Parameters.Select(p => p.ToString()))})";
    }
}