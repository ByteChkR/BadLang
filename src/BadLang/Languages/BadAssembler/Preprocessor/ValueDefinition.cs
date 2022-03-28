namespace BadAssembler.Preprocessor;

public class ValueDefinition : PreProcessorDefinition
{
    public ValueDefinition(string definitionName, string value)
    {
        DefinitionName = definitionName;
        Value = value;
    }

    public override string DefinitionName { get; }
    public string Value { get; }

    public override string Process(SourceReader reader, PreprocessorContext context)
    {
        return Value;
    }
}