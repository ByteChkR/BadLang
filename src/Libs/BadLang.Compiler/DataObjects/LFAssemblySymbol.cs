namespace LF.Compiler.DataObjects;

public readonly struct LFAssemblySymbol : IEquatable<LFAssemblySymbol>
{
    public readonly string[] Parts;

    public LFAssemblySymbol(string[] parts)
    {
        Parts = parts;
    }

    public override bool Equals(object? obj)
    {
        return obj is LFAssemblySymbol sym && Equals(sym);
    }

    public bool Equals(LFAssemblySymbol other)
    {
        return ToString() == other.ToString();
    }

    public static bool operator ==(LFAssemblySymbol a, LFAssemblySymbol b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(LFAssemblySymbol a, LFAssemblySymbol b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override string ToString()
    {
        return string.Join("::", Parts);
    }

    public static implicit operator LFAssemblySymbol(string name)
    {
        return new LFAssemblySymbol(name.Split("::"));
    }
}