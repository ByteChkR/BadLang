namespace LF.Compiler.DataObjects.Code.Instruction;

public class LFLiteralInstructionArgument : LFAssemblyInstructionArgument
{
    public readonly LFAssemblySymbol Value;

    public LFLiteralInstructionArgument(LFAssemblySymbol value)
    {
        Value = value;
    }

    public override byte[] GetValue(int size)
    {
        return new byte[size];
    }

    public override string ToString()
    {
        return Value.ToString()!;
    }
}