namespace LF.Compiler.DataObjects.Code.Instruction;

public class LFValueInstructionArgument : LFAssemblyInstructionArgument
{
    public readonly ulong Value;

    public LFValueInstructionArgument(ulong value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"0x{Value:X}";
    }

    public override byte[] GetValue(int size)
    {
        switch (size)
        {
            case 1:
                return new[] { (byte)Value };
            case 2:
                return BitConverter.GetBytes((ushort)Value);
            case 4:
                return BitConverter.GetBytes((uint)Value);
            case 8:
                return BitConverter.GetBytes(Value);
            default:
                throw new InvalidCastException($"Invalid size {size}");
        }
    }
}