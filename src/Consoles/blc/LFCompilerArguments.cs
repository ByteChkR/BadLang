using LFC.Commandline;

namespace LFC;

public class LFCompilerArguments
{
    [Switch("--throw", "-t", "Throw an exception if the compilation fails")]
    public bool ThrowExceptions { get; set; } = false;

    [Switch("--no-checks", "-nc", "Disable post compilation checks.")]
    public bool DoNotCheckOutput { get; set; } = false;

    [Switch("--no-empty", "-ne", "Do not write empty assemblies(if no code or data is present)")]
    public bool DoNotWriteEmpty { get; set; } = false;

    [Switch("--optimize", "-o", "Optimize the output assembly")]
    public bool Optimize { get; set; } = false;

    [Switch("--no-logs", "-nl", "Does not print any logs")]
    public bool NoLogs { get; set; } = false;

    [Switch("--overwrite", "-ow", "Overwrite files when creating a new project")]
    public bool ForceOverwrite { get; set; } = false;

    [Switch("--no-symbols", "-nsym", "Do not export Symbols from the compiled assembly")]
    public bool DoNotExportSymbols { get; set; } = false;
}