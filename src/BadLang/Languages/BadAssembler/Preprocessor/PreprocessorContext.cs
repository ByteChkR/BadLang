using BadAssembler.Exceptions;
using BadVM.Shared;
using BadVM.Shared.Logging;

namespace BadAssembler.Preprocessor;

public class PreprocessorContext
{
    private static LogMask s_LogMask = new LogMask("Preprocessor");
    private readonly List<PreProcessorDefinition> m_Definitions = new()
    {
        new DefineDefinition(),
        new UnDefineDefinition(),
        new IfDefinition(),
        new IfNotDefinition(),
    };


    private readonly List<SourceFile> m_SourceFiles = new();

    public bool IsDefined(string name)
    {
        return m_Definitions.Any(d => d.DefinitionName == name);
    }

    public PreProcessorDefinition? GetDefinition(string name)
    {
        return m_Definitions.FirstOrDefault(d => d.DefinitionName == name);
    }

    public void AddDefinition(PreProcessorDefinition definition)
    {
        if (IsDefined(definition.DefinitionName))
            throw new Exception($"Definition {definition.DefinitionName} already exists");
        m_Definitions.Add(definition);
    }

    public void RemoveDefinition(string name)
    {
        var definition = m_Definitions.FirstOrDefault(d => d.DefinitionName == name);
        if (definition == null) throw new Exception($"Definition {name} does not exist");
        m_Definitions.Remove(definition);
    }

    public IEnumerable<SourceFile> GetSources()
    {
        return m_SourceFiles;
    }

    public void ReadFile(string path)
    {
        var f = new SourceFile(path);

        ProcessSource(f);

        m_SourceFiles.Add(f);
    }


    private void ProcessSource(SourceFile file)
    {
        var reader = new SourceReader(file);
        reader.SkipUntil(x => x == '#' || x == '\0');
        while (!reader.IsEoF)
        {
            var idx = reader.CurrentIndex;
            reader.Eat('#');
            var def = reader.ParseWord();
            var definition = GetDefinition(def.StringValue);
            if (definition == null)
                throw new ParseException($"Unknown definition {def.StringValue}",
                    new SourceToken(file.FileName, reader.CurrentIndex));

            s_LogMask.LogMessage($"Processing definition {def.StringValue}");
            var newStr = definition.Process(reader, this);
            file.Replace(idx, reader.CurrentIndex - idx, newStr);
            reader.CurrentIndex = idx;
            reader.SkipUntil(x => x == '#' || x == '\0');
            
        }
    }
}