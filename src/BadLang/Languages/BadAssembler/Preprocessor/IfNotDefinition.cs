namespace BadAssembler.Preprocessor;

public class IfNotDefinition : IfDefinition
{
    public override string DefinitionName => "ifndef";
    public override bool TrueBody(string name, PreprocessorContext context)
    {
        return !context.IsDefined(name);
    }
}