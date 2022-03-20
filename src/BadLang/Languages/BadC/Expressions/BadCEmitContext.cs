using BadAssembler;

using BadC.Expressions.Values.Symbols;
using BadC.Functions;

using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadC.Expressions;

public class BadCEmitContext
{

    public readonly CodeSectionWriter Writer;
    public readonly FunctionInfo Function;
    public readonly PostSegmentParseTasks TaskList;
    public readonly bool AllowTemplateTypes;

    #region Public

    public BadCEmitContext( CodeSectionWriter writer, FunctionInfo function, PostSegmentParseTasks taskList, bool allowTemplateTypes )
    {
        Writer = writer;
        Function = function;
        TaskList = taskList;
        AllowTemplateTypes = allowTemplateTypes;
    }

    public BadCVariableDeclaration GetNamedVar( AssemblySymbol symbol )
    {
        return Function.GetNamedVar( symbol, Writer.AssemblyWriter );
    }

    #endregion

}
