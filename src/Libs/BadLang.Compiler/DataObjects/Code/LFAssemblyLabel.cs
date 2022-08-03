using LF.Compiler.Reader;

namespace LF.Compiler.DataObjects.Code;

public class LFAssemblyLabel
{
    public readonly int InstructionIndex;
    public readonly string Name;
    public readonly LFSourcePosition Position;

    public LFAssemblyLabel(string name, int instructionIndex, LFSourcePosition sourcePosition)
    {
        Name = name;
        InstructionIndex = instructionIndex;
        Position = sourcePosition;
    }
}