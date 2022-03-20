using BadVM.Shared;
using BadVM.Shared.AssemblyFormat.Serialization;

using Newtonsoft.Json;

namespace BadC.DebugSymbols;

public class DebugSymbolExporter : ICompilationExporter
{

    private readonly List < DebugSymbol > m_Symbols = new List < DebugSymbol >();

    #region Public

    public void AddSymbol( SourceToken token, string assemblyName, string sectionName, int offset )
    {
        m_Symbols.Add( new DebugSymbol( token, assemblyName, sectionName, offset ) );
    }

    public void Export( string outputBinary )
    {
        string symbolsFile = Path.ChangeExtension( outputBinary, ".sym.json" );

        File.WriteAllText( symbolsFile, JsonConvert.SerializeObject( m_Symbols, Formatting.Indented ) );
    }

    #endregion

}
