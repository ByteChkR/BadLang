namespace LF.Compiler.Reader;

public static class LFReaderDataHelper
{
    private static readonly Dictionary<LFReaderDataSection, Dictionary<string, byte[]>> s_DataCache = new Dictionary<LFReaderDataSection, Dictionary<string, byte[]>>();

    private static string? IsCached(this LFReaderDataSection section, byte[] data)
    {
        if (!s_DataCache.ContainsKey(section))
        {
            return null;
        }

        Dictionary<string, byte[]> cache = s_DataCache[section];
        foreach (KeyValuePair<string, byte[]> pair in cache)
        {
            if (pair.Value.Length == data.Length)
            {
                bool equals = true;
                for (int i = 0; i < data.Length; i++)
                {
                    if (pair.Value[i] != data[i])
                    {
                        equals = false;

                        break;
                    }
                }

                if (equals)
                {
                    return pair.Key;
                }
            }
        }

        return null;
    }

    public static string EmitData(this LFReaderDataSection section, byte[] data)
    {
        string? name = section.IsCached(data);

        if (name != null && LFCompilerOptimizationSettings.Instance.OptimizeStringLiterals)
        {
            return name;
        }

        name = section.MakeUniqueName("__LFC__~CSTRING~");
        if (!s_DataCache.ContainsKey(section))
        {
            s_DataCache[section] = new Dictionary<string, byte[]>();
        }

        s_DataCache[section].Add(name, data);
        section.EmitData(name, data);

        return name;
    }
}