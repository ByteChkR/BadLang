using BadC.Expressions;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadC.DebugSymbols;

public static class DebugSymbolExtensions
{

    #region Public

    public static void AddSymbol( this BadCEmitContext context, ISectionWriter sectionWriter, SourceToken token )
    {
        DebugSymbolExporter exporter = context.Writer.AssemblyWriter.CompilationData.GetData < DebugSymbolExporter >();

        exporter.AddSymbol(
                           token,
                           context.Writer.AssemblyWriter.Name,
                           sectionWriter.SectionName,
                           sectionWriter.CurrentSize
                          );
    }

    #endregion

}
