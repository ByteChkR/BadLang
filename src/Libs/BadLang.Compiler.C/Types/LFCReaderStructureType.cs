using LF.Compiler.C.Functions;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Types;

public class LFCReaderStructureType
{
    private readonly LFReaderCodeSection m_ContainingSection;
    private readonly List<LFCFunction> m_Functions;
    private readonly List<LFCFunction> m_OperatorOverrides;
    private readonly Dictionary<string, LFCTypeToken> m_Properties;
    public readonly LFCTypeToken Name;
    public readonly LFSourcePosition Position;

    public LFCReaderStructureType(
        LFSourcePosition position,
        LFCTypeToken name,
        Dictionary<string, LFCTypeToken> properties,
        List<LFCFunction> functions,
        LFReaderCodeSection containingSection,
        List<LFCFunction> operatorOverrides)
    {
        Name = name;
        m_Properties = properties;
        m_Functions = functions;
        m_ContainingSection = containingSection;
        m_OperatorOverrides = operatorOverrides;
        Position = position;
    }

    public LFCStructureType CreateType(LFCTypeDatabase db, Dictionary<LFCTypeToken, LFCTypeToken> typeArgs)
    {
        LFCTypeToken name = LFCScope.ResolveType(Name, typeArgs);
        Dictionary<string, LFCType> members = new Dictionary<string, LFCType>();
        LFCStructureType ret = new LFCStructureType(name, members, m_ContainingSection, m_OperatorOverrides);
        db.AddType(ret);
        foreach (KeyValuePair<string, LFCTypeToken> member in m_Properties)
        {
            LFCType memberType = LFCScope.GetType(member.Value, db, typeArgs, Position);
            members.Add(member.Key, memberType);
        }

        foreach (LFCFunction function in m_Functions)
        {
            LFCFunctionType fType = LFCFunctionType.GetType(function);
            LFCFunctionType funcType = (LFCFunctionType)LFCScope.GetType(fType.Name, db, typeArgs, Position);
            funcType.SetSource(fType.SourceFunction);
            members.Add(function.Name, funcType);
        }

        return ret;
    }
}