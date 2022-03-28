using BadAssembler.Exceptions;
using BadVM.Shared;

namespace BadAssembler.Preprocessor;

public class DefineDefinition : PreProcessorDefinition
{
    public override string DefinitionName => "define";

    public override string Process(SourceReader reader, PreprocessorContext context)
    {
        reader.SkipWhitespaceAndNewlineAndComments("//");
        var name = reader.ParseWord();
        if (string.IsNullOrEmpty(name.StringValue))
            throw new ParseException("Expected identifier after define", name.SourceToken);

        reader.SkipWhitespaceAndNewlineAndComments("//");

        if (reader.Is('('))
        {
            //parse function signature and block
            reader.Eat('(');
            var parameters = new List<string>();
            while (true)
            {
                if (reader.Is(')'))
                {
                    reader.Eat(')');
                    break;
                }

                reader.SkipWhitespaceAndNewlineAndComments("//");

                parameters.Add(reader.ParseWord().StringValue);
                reader.SkipWhitespaceAndNewlineAndComments("//");

                if (reader.Is(',')) reader.Eat(',');
                else if (!reader.Is(')'))
                    throw new ParseException("Expected ')' or ','",
                        new SourceToken(reader.SourceFile.FileName, reader.CurrentIndex));
            }

            reader.SkipWhitespaceAndNewlineAndComments("//");
            if (!reader.Is('{'))
                throw new ParseException("Expected '{'",
                    new SourceToken(reader.SourceFile.FileName, reader.CurrentIndex));

            reader.Eat('{');
            reader.SkipWhitespaceAndNewlineAndComments("//");

            var functionBody = reader.ReadToEndOfBlock('{', '}', "//").Trim();

            context.AddDefinition(new FunctionDefinition(name.StringValue, parameters.ToArray(), functionBody));

            reader.SkipWhitespaceAndNewlineAndComments("//");
        }
        else
        {
            reader.SkipWhitespaceAndNewlineAndComments("//");
            var value = reader.ParseUntilWhiteSpace();
            context.AddDefinition(new ValueDefinition(name.StringValue, value.StringValue));
            reader.SkipWhitespaceAndNewlineAndComments("//");
        }

        return "";
    }
}