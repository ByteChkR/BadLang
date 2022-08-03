using LF.Compiler.Logging;

namespace LF.Compiler.Reader;

public abstract class LFCodeParser
{
    private static readonly Dictionary<string, LFCodeParser> s_FileExtensions = new Dictionary<string, LFCodeParser>();

    public static void AddParser<T>(string extension) where T : LFCodeParser, new()
    {
        AddParser(extension, new T());
    }

    public static void AddParser(string extension, LFCodeParser parser)
    {
        s_FileExtensions.Add(extension, parser);
    }

    protected abstract LFCodeReader CreateReader(string src, string workingDir, string fileName);

    private static LFCodeReader FromExtension(string file, string workingDir)
    {
        file = Path.GetFullPath(file);
        string ext = Path.GetExtension(file);

        if (!s_FileExtensions.ContainsKey(ext))
        {
            throw new Exception($"No parser for extension {ext}");
        }

        return s_FileExtensions[ext].CreateReader(File.ReadAllText(file), workingDir, file);
    }

    public static LFReaderResult Parse(string name, params string[] files)
    {
        LFReaderResult result = new LFReaderResult(name);

        foreach (string file in files)
        {
            result.AddInclude(Path.GetFullPath(file));
        }

        string[] includes = result.GetIncludedFiles();
        result.ClearIncludes();
        while (includes.Length > 0)
        {
            foreach (string file in includes)
            {
                if (!File.Exists(file))
                {
                    throw new Exception($"File not found: {file}");
                }

                Logger.Info($"Compiling: {file}");
                string wDir = Path.GetDirectoryName(Path.GetFullPath(file))!;
                LFCodeReader reader = FromExtension(file, wDir);

                reader.ReadToEnd(result);
            }

            includes = result.GetIncludedFiles();
            result.ClearIncludes();
        }

        result.RunFinalize();

        return result;
    }
}