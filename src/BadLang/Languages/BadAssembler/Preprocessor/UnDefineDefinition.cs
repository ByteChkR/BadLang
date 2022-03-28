namespace BadAssembler.Preprocessor;

public class UnDefineDefinition : PreProcessorDefinition
{
    public override string DefinitionName => "undefine";

    public override string Process(SourceReader reader, PreprocessorContext context)
    {
        reader.SkipWhitespaceAndNewlineAndComments("//");
        var name = reader.ParseWord();

        context.RemoveDefinition(name.StringValue);

        return "";
    }
}