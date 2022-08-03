using LF.Compiler.C.Types;

namespace LF.Compiler.C.Functions;

public class LFCFunctionType : LFCType, IEquatable<LFCFunctionType>
{
    private readonly LFCTypeToken[] m_ParameterTypeNames;
    private readonly LFCTypeToken[] m_TypeParameterNames;
    public readonly LFCTypeToken ReturnTypeName;
    public LFCFunction? SourceFunction { get; private set; }
    public void SetSource(LFCFunction? function) => SourceFunction = function;

    public LFCFunctionType(
        LFCTypeToken returnType,
        LFCTypeToken[] parameterTypes,
        LFCTypeToken[] typeParameters,
        LFCFunction? sourceFunction = null) : base(
        GetName(returnType, parameterTypes, typeParameters),
        null
    )
    {
        m_TypeParameterNames = typeParameters;
        SourceFunction = sourceFunction;
        m_ParameterTypeNames = parameterTypes;
        ReturnTypeName = returnType;
    }

    public bool IsInstance => SourceFunction?.IsInstance ?? false;

    public IEnumerable<LFCTypeToken> TypeParameters => m_TypeParameterNames;

    public bool HasTypeParameters => m_TypeParameterNames.Length != 0;
    public int TypeParameterCount => m_TypeParameterNames.Length;
    public int ParameterCount => m_ParameterTypeNames.Length;

    public bool Equals(LFCFunctionType? funcType)
    {
        if (funcType != null &&
            funcType.Name == Name &&
            funcType.ReturnTypeName == ReturnTypeName &&
            funcType.m_ParameterTypeNames.Length == m_ParameterTypeNames.Length &&
            funcType.m_TypeParameterNames.Length == m_TypeParameterNames.Length)
        {
            for (int i = 0; i < m_ParameterTypeNames.Length; i++)
            {
                if (m_ParameterTypeNames[i] != funcType.m_ParameterTypeNames[i])
                {
                    return false;
                }
            }

            for (int i = 0; i < m_TypeParameterNames.Length; i++)
            {
                if (m_TypeParameterNames[i] != funcType.m_TypeParameterNames[i])
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        int code = HashCode.Combine(base.GetHashCode(), ReturnTypeName.GetHashCode());

        foreach (LFCTypeToken param in m_ParameterTypeNames)
        {
            code = HashCode.Combine(code, param.GetHashCode());
        }

        foreach (LFCTypeToken typeParameterName in m_TypeParameterNames)
        {
            code = HashCode.Combine(code, typeParameterName.GetHashCode());
        }

        return code;
    }

    public override bool Equals(object? obj)
    {
        if (obj is LFCFunctionType funcType)
        {
            return Equals(funcType);
        }

        return false;
    }


    public static bool operator ==(LFCFunctionType? left, LFCFunctionType? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(LFCFunctionType? left, LFCFunctionType? right)
    {
        return !Equals(left, right);
    }

    public LFCTypeToken GetParameterType(int i)
    {
        return m_ParameterTypeNames[i];
    }

    public static LFCFunctionType GetType(LFCFunction func)
    {
        return GetType(
            func,
            func.ReturnTypeName,
            func.Parameters.Select(x => x.TypeName).ToArray(),
            func.TypeParameters.ToArray()
        );
    }

    private static LFCFunctionType GetType(
        LFCFunction func,
        LFCTypeToken returnType,
        LFCTypeToken[] parameterTypes,
        LFCTypeToken[] typeArgs)
    {
        return new LFCFunctionType(returnType, parameterTypes, typeArgs, func);
    }

    private static LFCTypeToken GetName(LFCTypeToken returnType, LFCTypeToken[] parameterTypes, LFCTypeToken[] typeParameters)
    {
        List<LFCTypeToken> p = new List<LFCTypeToken>();
        p.Add(returnType);
        p.AddRange(parameterTypes);
        return new LFCTypeToken("Function", p.ToArray());
    }


    public override int GetSize()
    {
        return 8;
    }

    public override IEnumerable<(string name, LFCType type)> GetFields()
    {
        yield break;
    }

    public override IEnumerable<(string name, LFCFunctionType type)> GetMethods()
    {
        yield break;
    }
}