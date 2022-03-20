using BadC.Utils;

using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadC.Functions;

public static class FunctionPrologueWriter
{

    #region Public

    public static void WriteProlouge( this FunctionInfo func, CodeSectionWriter writer )
    {
        writer.Label( func.Symbol.SymbolName, func.Visibility );

        //Make Room for all locals

        int localSize = func.TotalLocalSize;

        writer.Push( localSize );
    }

    #endregion

}
