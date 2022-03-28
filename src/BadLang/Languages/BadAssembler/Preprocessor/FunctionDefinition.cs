using BadAssembler.Exceptions;
using BadVM.Shared;

namespace BadAssembler.Preprocessor;

public class FunctionDefinition : PreProcessorDefinition
{
    private readonly string m_Body;
    private readonly string[] m_Parameters;

    public FunctionDefinition(string definitionName, string[] parameters, string body)
    {
        DefinitionName = definitionName;
        m_Parameters = parameters;
        m_Body = body;
    }


    public override string DefinitionName { get; }

    public override string Process(SourceReader reader, PreprocessorContext context)
    {
        //Parse signature
        reader.Eat('(');

        var parameter = reader.ParseList();

        reader.Eat(')');

        if (m_Parameters.Length != parameter.Length)
            throw new ParseException("Function signature does not match",
                new SourceToken(reader.SourceFile.FileName, reader.CurrentIndex));

        var body = m_Body;
        for (var i = 0; i < m_Parameters.Length; i++)
        {
            var currentIndex = body.IndexOf(m_Parameters[i], 0, StringComparison.InvariantCulture);
            while (currentIndex != -1)
            {
                if (currentIndex > 0 && char.IsLetter(body[currentIndex - 1]) ||
                    currentIndex+m_Parameters[i].Length < body.Length - 1 && char.IsLetter(body[currentIndex+m_Parameters[i].Length]))
                {
                    currentIndex = body.IndexOf(m_Parameters[i], currentIndex + 1, StringComparison.InvariantCulture);
                    continue;
                }

                body = body.Remove(currentIndex, m_Parameters[i].Length);
                body = body.Insert(currentIndex, parameter[i]);
                currentIndex = body.IndexOf(m_Parameters[i], currentIndex + parameter[i].Length,
                    StringComparison.InvariantCulture);
            }
        }

        return body;
    }
}