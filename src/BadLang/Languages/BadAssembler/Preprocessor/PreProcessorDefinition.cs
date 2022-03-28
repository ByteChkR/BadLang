namespace BadAssembler.Preprocessor;

public abstract class PreProcessorDefinition
{
    public abstract string DefinitionName { get; }

    public abstract string Process(SourceReader reader, PreprocessorContext context);
}