namespace LF.Compiler.DataObjects.Code.Instruction;

public abstract class LFAssemblyInstructionArgument
{
    public abstract byte[] GetValue(int size);

    public static implicit operator LFAssemblyInstructionArgument(ulong d)
    {
        return new LFValueInstructionArgument(d);
    }

    public static implicit operator LFAssemblyInstructionArgument(string name)
    {
        return new LFLiteralInstructionArgument(name);
    }
}