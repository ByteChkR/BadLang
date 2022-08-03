using LF.Compiler.C.Functions;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Types;

public abstract class LFCType
{
    public readonly LFCType? BaseType;

    public readonly LFCTypeToken Name;

    protected LFCType(LFCTypeToken name, LFCType? baseType)
    {
        Name = name;
        BaseType = baseType;
    }

    public bool HasConstructor => HasProperty(LFCStructureType.GetFunctionName(Name.Name, ".ctor"));

    public bool HasBaseType => BaseType != null;

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, BaseType);
    }

    public virtual int GetSize()
    {
        return BaseType?.GetSize() ?? 0;
    }

    public abstract IEnumerable<(string name, LFCType type)> GetFields();
    public abstract IEnumerable<(string name, LFCFunctionType type)> GetMethods();
    public virtual bool HasProperty(string name)
    {
        return false;
    }

    public virtual LFCType GetPropertyType(string name, LFSourcePosition position)
    {
        throw new LFCParserException("Property not found", position);
    }

    public virtual int GetPropertyOffset(string name, LFSourcePosition position)
    {
        return 0;
    }


    public override string ToString()
    {
        return Name.ToString()!;
    }

    public static bool operator ==(LFCType? a, LFCType? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        return a?.Equals(b) ?? b?.Equals(a) ?? false;
    }

    public static bool operator !=(LFCType? a, LFCType? b)
    {
        return !(a == b);
    }


    public virtual bool HasOverride(string name, params LFCType[] args)
    {
        return false;
    }

    public virtual LFCFunctionType GetOverride(LFSourcePosition position, string name, params LFCType[] args)
    {
        throw new LFCParserException($"Method '{name}' not found in type {Name}", position);
    }
}