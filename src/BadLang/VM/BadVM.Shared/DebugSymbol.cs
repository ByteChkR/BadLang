namespace BadVM.Shared;

public readonly struct DebugSymbol : IEquatable<DebugSymbol>
{

    public readonly SourceToken SourceToken;
    public readonly string AssemblyName;
    public readonly string SectionName;
    public readonly long SectionOffset;

    public DebugSymbol( SourceToken sourceToken, string assemblyName, string sectionName, int sectionOffset )
    {
        SourceToken = sourceToken;
        AssemblyName = assemblyName;
        SectionName = sectionName;
        SectionOffset = sectionOffset;
    }

    public bool Equals(DebugSymbol other)
    {
        return SourceToken.Equals(other.SourceToken) && AssemblyName == other.AssemblyName && SectionName == other.SectionName && SectionOffset == other.SectionOffset;
    }

    public override bool Equals(object? obj)
    {
        return obj is DebugSymbol other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = SourceToken.GetHashCode();
            hashCode = (hashCode * 397) ^ AssemblyName.GetHashCode();
            hashCode = (hashCode * 397) ^ SectionName.GetHashCode();
            hashCode = (hashCode * 397) ^ SectionOffset.GetHashCode();
            return hashCode;
        }
    }
}
