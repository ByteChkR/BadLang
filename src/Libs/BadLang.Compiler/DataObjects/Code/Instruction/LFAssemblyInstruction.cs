using LF.Compiler.Reader;

namespace LF.Compiler.DataObjects.Code.Instruction;

public class LFAssemblyInstruction
{
    public readonly LFAssemblyInstructionArgument[] Arguments;
    public readonly OpCodes OpCode;
    public readonly LFSourcePosition SourcePosition;

    public LFAssemblyInstruction(LFSourcePosition sourcePosition, OpCodes opCode, params LFAssemblyInstructionArgument[] arguments)
    {
        OpCode = opCode;
        Arguments = arguments;
        SourcePosition = sourcePosition;
    }

    public override string ToString()
    {
        return $"{OpCode} {string.Join(", ", Arguments.Select(x => x.ToString()))}";
    }
}