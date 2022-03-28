namespace BadAssembler.Preprocessor;

public class IfDefinition : PreProcessorDefinition
{
    public override string DefinitionName => "ifdef";

    public virtual bool TrueBody(string name, PreprocessorContext context)
    {
        return context.IsDefined(name);
    }
    public override string Process(SourceReader reader, PreprocessorContext context)
    {
        reader.SkipWhitespaceAndNewlineAndComments("//");
        var name = reader.ParseWord();

        reader.SkipWhitespaceAndNewlineAndComments("//");
        reader.Eat('{');
        reader.SkipWhitespaceAndNewlineAndComments("//");

        var trueBody = reader.ReadToEndOfBlock('{', '}', "//").Trim();
        
        reader.SkipWhitespaceAndNewlineAndComments("//");
        //reader.Eat('}');

        reader.SkipWhitespaceAndNewlineAndComments("//");
        var falseBody = "";
        if (reader.Is("#else"))
        {
            reader.Eat("#else");
            reader.SkipWhitespaceAndNewlineAndComments("//");
            reader.Eat('{');
            reader.SkipWhitespaceAndNewlineAndComments("//");
            falseBody = reader.ReadToEndOfBlock('{', '}', "//").Trim();
        }

        if (TrueBody(name.StringValue, context)) 
            return trueBody;
        return falseBody;
    }
}