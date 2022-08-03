namespace LF.Compiler;

public class LFCompilerOptimizationSettings
{
    public static readonly LFCompilerOptimizationSettings Instance = new LFCompilerOptimizationSettings();
    private LFCompilerOptimizationSettings() { }

    public bool OptimizeAll
    {
        set => OptimizePop = OptimizePush = OptimizeOffsetCalculation = OptimizeLocalCalls = OptimizeLocalJumps = OptimizeStringLiterals = value;
    }

    public bool OptimizePush { get; set; }
    public bool OptimizePop { get; set; }
    public bool OptimizeOffsetCalculation { get; set; }
    public bool OptimizeLocalJumps { get; set; }
    public bool OptimizeLocalCalls { get; set; }
    public bool OptimizeStringLiterals { get; set; }
}