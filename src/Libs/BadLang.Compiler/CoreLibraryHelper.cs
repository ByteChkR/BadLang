using LF.Compiler.Logging;

namespace LF.Compiler;

public static class CoreLibraryHelper
{
    public static string CoreLibraryDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "projects");

    public static string GetCoreLibrary(string name)
    {
        string path = Path.Combine(CoreLibraryDirectory, name, "bin", $"{name}.lfbin");
        if (!File.Exists(path))
        {
            Logger.Error($"Core library not found: {name} (did you forget to build it?)");

            throw new Exception($"Core library not found: {name} (did you forget to build it?)");
        }

        return path;
    }
}