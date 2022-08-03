using LF.Compiler.C.Functions;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Types;

public class LFCStructureType : LFCType
{
    public readonly LFReaderCodeSection ContainingSection;
    private readonly Dictionary<string, LFCType> m_Members;

    private readonly List<LFCFunction> m_OperatorOverrides;

    public LFCStructureType(LFCTypeToken name, Dictionary<string, LFCType> members, LFReaderCodeSection containingSection, List<LFCFunction> operatorOverrides) : base(name, null)
    {
        m_Members = members;
        ContainingSection = containingSection;
        m_OperatorOverrides = operatorOverrides;
    }

    public static string GetFunctionName(string instance, string name)
    {
        return instance + "_" + name;
    }

    public override IEnumerable<(string name, LFCFunctionType type)> GetMethods()
    {
        return m_Members.Where(x => x.Value is LFCFunctionType funcType && funcType.SourceFunction != null).Select(x => (x.Key, (LFCFunctionType)x.Value));
    }

    public override bool HasProperty(string name)
    {
        return m_Members.ContainsKey(name);
    }

    public override LFCType GetPropertyType(string name, LFSourcePosition position)
    {
        return m_Members[name];
    }

    public override int GetPropertyOffset(string name, LFSourcePosition position)
    {
        int current = 0;
        foreach (KeyValuePair<string, LFCType> member in m_Members)
        {
            if (member.Key == name)
            {
                return current;
            }

            current += member.Value.GetSize();
        }

        throw new LFCParserException($"Member '{name}' not found in structure '{Name}'", position);
    }

    public override int GetSize()
    {
        int size = 0;
        foreach (KeyValuePair<string, LFCType> member in m_Members)
        {
            if (member.Value is LFCFunctionType funcType && funcType.SourceFunction != null)
            {
                continue;
            }

            size += member.Value.GetSize();
        }

        return size;
    }

    public override IEnumerable<(string name, LFCType type)> GetFields()
    {
        return m_Members.Where(x => !(x.Value is LFCFunctionType funcType && funcType.SourceFunction != null)).Select(x => (x.Key, x.Value));
    }

    private bool IsOperator(LFCFunction func, string name, LFCType[] args)
    {
        if (func.ParameterCount != args.Length || func.Name != Name + "_" + name + "_" + string.Join('_', args.Select(x => x.Name.ToString())))
        {
            return false;
        }

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Name != func.GetParameter(i))
            {
                return false;
            }
        }

        return true;
    }

    public override bool HasOverride(string name, params LFCType[] args)
    {
        LFCFunction? func = m_OperatorOverrides.FirstOrDefault(x => IsOperator(x, name, args));

        return func != null;
    }

    public override LFCFunctionType GetOverride(LFSourcePosition position, string name, params LFCType[] args)
    {
        LFCFunction? func = m_OperatorOverrides.FirstOrDefault(x => IsOperator(x, name, args));

        if (func == null)
        {
            return base.GetOverride(position, name, args);
        }

        return LFCFunctionType.GetType(func);
    }
}