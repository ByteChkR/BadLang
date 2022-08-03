using System.Text;

namespace LF.Compiler.Debugging;

public class LFAssemblyDebugSymbolDatabase
{
    private readonly List<LFAssemblyDebugSymbol> m_Symbols = new List<LFAssemblyDebugSymbol>();

    public void AddSymbol(LFAssemblyDebugSymbol symbol)
    {
        m_Symbols.Add(symbol);
    }

    public string Export()
    {
        StringBuilder sb = new StringBuilder();
        foreach (LFAssemblyDebugSymbol symbol in m_Symbols)
        {
            sb.AppendLine(symbol.Export());
        }

        return sb.ToString();
    }
}