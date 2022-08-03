using LF.Compiler.C.Functions;

namespace LF.Compiler.C.Types;

public class LFCBaseType : LFCType
{
    private static readonly List<LFCBaseType> s_BaseTypes = new List<LFCBaseType>();
    private readonly int m_Size;

    static LFCBaseType()
    {
        s_BaseTypes.Add(new LFCBaseType("void", 0));
        s_BaseTypes.Add(new LFCBaseType("i8", 1));
        s_BaseTypes.Add(new LFCBaseType("i16", 2));
        s_BaseTypes.Add(new LFCBaseType("i32", 4));
        s_BaseTypes.Add(new LFCBaseType("i64", 8));
    }

    public LFCBaseType(string name, int size) : this(new LFCTypeToken(name, Array.Empty<LFCTypeToken>()), size) { }

    public LFCBaseType(LFCTypeToken name, int size) : base(name, null)
    {
        m_Size = size;
    }

    public static IEnumerable<LFCBaseType> BaseTypes => s_BaseTypes;

    public static LFCBaseType GetType(string name)
    {
        return GetType(new LFCTypeToken(name, Array.Empty<LFCTypeToken>()));
    }

    public static LFCBaseType GetType(LFCTypeToken name)
    {
        return s_BaseTypes.First(x => x.Name == name);
    }

    public static bool IsBaseType(LFCType type)
    {
        return type is LFCBaseType bType && s_BaseTypes.Contains(bType);
    }

    public override int GetSize()
    {
        return base.GetSize() + m_Size;
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