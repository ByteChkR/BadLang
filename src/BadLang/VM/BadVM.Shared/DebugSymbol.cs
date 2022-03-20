namespace BadVM.Shared;

public struct DebugSymbol
{

    public SourceToken SourceToken;
    public string AssemblyName;
    public string SectionName;
    public long SectionOffset;

    public DebugSymbol( SourceToken sourceToken, string assemblyName, string sectionName, int sectionOffset )
    {
        SourceToken = sourceToken;
        AssemblyName = assemblyName;
        SectionName = sectionName;
        SectionOffset = sectionOffset;
    }

}
