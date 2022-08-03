using LF.Compiler.C.Functions;

namespace LF.Compiler.C.Types;

public class LFCPointerType : LFCType, IEquatable<LFCPointerType>
{
    public readonly LFCType ElementType;

    private LFCPointerType(LFCTypeToken name, LFCType elementType) : base(name, null)
    {
        ElementType = elementType;
    }

    public bool Equals(LFCPointerType? other)
    {
        return other is not null && ElementType.Equals(other.ElementType);
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

    public static LFCPointerType Create(LFCType type)
    {
        return new LFCPointerType(new LFCTypeToken($"{type.Name.Name}*", type.Name.TypeArgs), type);
    }

    public override bool Equals(object? obj)
    {
        return obj is LFCPointerType type && Equals(type);
    }

    public override int GetHashCode()
    {
        return (int)(ElementType.GetHashCode() ^ 0xA5A5A5A5);
    }
}