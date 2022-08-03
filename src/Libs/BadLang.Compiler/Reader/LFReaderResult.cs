using System.Text;

using LF.Compiler.DataObjects.Code;
using LF.Compiler.Debugging;

namespace LF.Compiler.Reader;

public class LFReaderResult
{
    private readonly List<LFReaderCodeSection> m_CodeSections = new List<LFReaderCodeSection>();
    private readonly List<LFReaderDataSection> m_DataSections = new List<LFReaderDataSection>();
    private readonly List<string> m_IncludedFiles = new List<string>();

    private readonly List<Action> m_OnFinalize = new List<Action>();
    private readonly List<Action> m_BeforeFinalize = new List<Action>();
    private readonly List<Action> m_AfterFinalize = new List<Action>();
    private readonly List<string> m_Requires = new List<string>();
    public readonly string Name;

    public LFReaderResult(string name)
    {
        Name = name;
    }

    public bool IsEmpty =>
        m_IncludedFiles.Count == 0 &&
        m_CodeSections.All(x => x.IsEmpty) &&
        m_DataSections.All(x => x.IsEmpty);

    public IEnumerable<LFReaderCodeSection> CodeSections => m_CodeSections;
    public IEnumerable<LFReaderDataSection> DataSections => m_DataSections;

    public string[] GetIncludedFiles()
    {
        return m_IncludedFiles.ToArray();
    }

    public void ClearIncludes()
    {
        m_IncludedFiles.Clear();
    }

    public LFReaderCodeSection CreateCodeSection(string name)
    {
        LFReaderCodeSection section = new LFReaderCodeSection(name);
        m_CodeSections.Add(section);

        return section;
    }

    public LFReaderDataSection CreateDataSection(string name)
    {
        LFReaderDataSection section = new LFReaderDataSection(name);
        m_DataSections.Add(section);

        return section;
    }

    public void AddFileRequire(string file)
    {
        m_Requires.Add($"file://{file}");
    }

    public void AddNameRequire(string name)
    {
        m_Requires.Add($"name://{name}");
    }

    public void AddInclude(string file)
    {
        m_IncludedFiles.Add(file);
    }

    private byte[] GetRequiredBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(m_Requires.Count));
        foreach (string require in m_Requires)
        {
            byte[] requireBytes = Encoding.UTF8.GetBytes(require);
            bytes.AddRange(BitConverter.GetBytes(requireBytes.Length));
            bytes.AddRange(requireBytes);
        }

        return bytes.ToArray();
    }

    public void Serialize(Stream s, LFAssemblyDebugSymbolDatabase symbolDatabase)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(Name);
        s.Write(BitConverter.GetBytes(nameBytes.Length));
        s.Write(nameBytes, 0, nameBytes.Length);

        byte[] requiredBytes = GetRequiredBytes();
        s.Write(BitConverter.GetBytes(requiredBytes.Length));
        s.Write(requiredBytes, 0, requiredBytes.Length);


        s.Write(BitConverter.GetBytes(m_CodeSections.Count));

        foreach (LFReaderCodeSection section in m_CodeSections)
        {
            section.Serialize(s, symbolDatabase);
        }

        s.Write(BitConverter.GetBytes(m_DataSections.Count));

        foreach (LFReaderDataSection section in m_DataSections)
        {
            section.Serialize(s);
        }
    }

    public bool HasLabel(string label, LFReaderCodeSection from)
    {
        foreach (LFAssemblyLabel lfAssemblyLabel in from.Labels)
        {
            if (lfAssemblyLabel.Name == label)
            {
                return true;
            }
        }

        foreach (LFReaderCodeSection section in m_CodeSections)
        {
            foreach (LFAssemblyLabel lfAssemblyLabel in section.Labels)
            {
                string name = section.Name + "::" + lfAssemblyLabel.Name;

                if (label == name)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void OnFinalize(Action a)
    {
        m_OnFinalize.Add(a);
    }

    public void AfterFinalize(Action a)
    {
        m_AfterFinalize.Add(a);
    }
    public void BeforeFinalize(Action a)
    {
        m_BeforeFinalize.Add(a);
    }

    public void RunFinalize()
    {
        for (int i = 0; i < m_BeforeFinalize.Count; i++)
        {
            m_BeforeFinalize[i]();
        }

        m_BeforeFinalize.Clear();
        
        for (int i = 0; i < m_OnFinalize.Count; i++)
        {
            m_OnFinalize[i]();
        }

        m_OnFinalize.Clear();
        
        for (int i = 0; i < m_AfterFinalize.Count; i++)
        {
            m_AfterFinalize[i]();
        }

        m_AfterFinalize.Clear();
    }

    public LFReaderCodeSection GetCodeSection(string name)
    {
        foreach (LFReaderCodeSection section in m_CodeSections)
        {
            if (section.Name == name)
            {
                return section;
            }
        }

        throw new Exception($"Code section not found: {name}");
    }

    public LFReaderDataSection GetDataSection(string name)
    {
        foreach (LFReaderDataSection section in m_DataSections)
        {
            if (section.Name == name)
            {
                return section;
            }
        }

        throw new Exception($"Data section not found: {name}");
    }

    public LFReaderDataSection GetOrCreateDataSection(string name)
    {
        foreach (LFReaderDataSection section in m_DataSections)
        {
            if (section.Name == name)
            {
                return section;
            }
        }

        LFReaderDataSection cSection = new LFReaderDataSection(name);
        m_DataSections.Add(cSection);

        return cSection;
    }

    public LFReaderCodeSection GetOrCreateCodeSection(string name)
    {
        foreach (LFReaderCodeSection section in m_CodeSections)
        {
            if (section.Name == name)
            {
                return section;
            }
        }

        LFReaderCodeSection cSection = new LFReaderCodeSection(name);
        m_CodeSections.Add(cSection);

        return cSection;
    }
}