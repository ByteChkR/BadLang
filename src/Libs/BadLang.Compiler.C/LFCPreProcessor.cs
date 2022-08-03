using LF.Compiler.Reader;

namespace LF.Compiler.C;

public class LFCPreProcessor
{
    private static readonly Dictionary<LFReaderResult, LFCPreProcessor> s_PreProcessors = new Dictionary<LFReaderResult, LFCPreProcessor>();

    private readonly Dictionary<string, string> m_Defines = new Dictionary<string, string>
    {
        { "LFC_PROJECTS", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "projects") },
        { "LFC_TEMPLATES", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "templates") },
    };

    private readonly Dictionary<string, LFCPreProcessorMacro> m_Macros = new Dictionary<string, LFCPreProcessorMacro>();

    public IEnumerable<string> Defines => m_Defines.Keys;
    public IEnumerable<string> Macros => m_Macros.Keys;

    public static LFCPreProcessor GetPreProcessor(LFReaderResult readerResult)
    {
        if (!s_PreProcessors.ContainsKey(readerResult))
        {
            s_PreProcessors.Add(readerResult, new LFCPreProcessor());
        }

        return s_PreProcessors[readerResult];
    }

    public void AddMacro(string name, LFCPreProcessorMacro macro)
    {
        m_Macros.Add(name, macro);
    }

    public bool HasMacro(string name)
    {
        return m_Macros.ContainsKey(name);
    }

    public LFCPreProcessorMacro GetMacro(string name)
    {
        return m_Macros[name];
    }

    public void Define(string name, string value)
    {
        m_Defines.Add(name, value);
    }

    public void UnDefine(string name)
    {
        m_Defines.Remove(name);
    }

    public bool IsDefined(string name)
    {
        return m_Defines.ContainsKey(name);
    }

    public string GetDefine(string name)
    {
        return m_Defines[name];
    }
}