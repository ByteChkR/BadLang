namespace LF.Compiler.DataObjects.Code.Instruction;

public class LFAssemblyInstructionInfo
{
    private static readonly Dictionary<OpCodes, LFAssemblyInstructionInfo> s_InstructionInfos =
        new Dictionary<OpCodes, LFAssemblyInstructionInfo>
        {
            { OpCodes.Nop, new LFAssemblyInstructionInfo() },
            { OpCodes.Halt, new LFAssemblyInstructionInfo() },
            { OpCodes.Push8, new LFAssemblyInstructionInfo(1) },
            { OpCodes.Push16, new LFAssemblyInstructionInfo(2) },
            { OpCodes.Push32, new LFAssemblyInstructionInfo(4) },
            { OpCodes.Push64, new LFAssemblyInstructionInfo(8) },
            { OpCodes.PushSF, new LFAssemblyInstructionInfo() },
            { OpCodes.MoveSP, new LFAssemblyInstructionInfo(4) },
            { OpCodes.Pop8, new LFAssemblyInstructionInfo() },
            { OpCodes.Pop16, new LFAssemblyInstructionInfo() },
            { OpCodes.Pop32, new LFAssemblyInstructionInfo() },
            { OpCodes.Pop64, new LFAssemblyInstructionInfo() },
            { OpCodes.Dup8, new LFAssemblyInstructionInfo() },
            { OpCodes.Dup16, new LFAssemblyInstructionInfo() },
            { OpCodes.Dup32, new LFAssemblyInstructionInfo() },
            { OpCodes.Dup64, new LFAssemblyInstructionInfo() },
            { OpCodes.Swap8, new LFAssemblyInstructionInfo() },
            { OpCodes.Swap16, new LFAssemblyInstructionInfo() },
            { OpCodes.Swap32, new LFAssemblyInstructionInfo() },
            { OpCodes.Swap64, new LFAssemblyInstructionInfo() },
            { OpCodes.Add8, new LFAssemblyInstructionInfo() },
            { OpCodes.Add16, new LFAssemblyInstructionInfo() },
            { OpCodes.Add32, new LFAssemblyInstructionInfo() },
            { OpCodes.Add64, new LFAssemblyInstructionInfo() },
            { OpCodes.Sub8, new LFAssemblyInstructionInfo() },
            { OpCodes.Sub16, new LFAssemblyInstructionInfo() },
            { OpCodes.Sub32, new LFAssemblyInstructionInfo() },
            { OpCodes.Sub64, new LFAssemblyInstructionInfo() },
            { OpCodes.Mul8, new LFAssemblyInstructionInfo() },
            { OpCodes.Mul16, new LFAssemblyInstructionInfo() },
            { OpCodes.Mul32, new LFAssemblyInstructionInfo() },
            { OpCodes.Mul64, new LFAssemblyInstructionInfo() },
            { OpCodes.Div8, new LFAssemblyInstructionInfo() },
            { OpCodes.Div16, new LFAssemblyInstructionInfo() },
            { OpCodes.Div32, new LFAssemblyInstructionInfo() },
            { OpCodes.Div64, new LFAssemblyInstructionInfo() },
            { OpCodes.Mod8, new LFAssemblyInstructionInfo() },
            { OpCodes.Mod16, new LFAssemblyInstructionInfo() },
            { OpCodes.Mod32, new LFAssemblyInstructionInfo() },
            { OpCodes.Mod64, new LFAssemblyInstructionInfo() },
            { OpCodes.And8, new LFAssemblyInstructionInfo() },
            { OpCodes.And16, new LFAssemblyInstructionInfo() },
            { OpCodes.And32, new LFAssemblyInstructionInfo() },
            { OpCodes.And64, new LFAssemblyInstructionInfo() },
            { OpCodes.Or8, new LFAssemblyInstructionInfo() },
            { OpCodes.Or16, new LFAssemblyInstructionInfo() },
            { OpCodes.Or32, new LFAssemblyInstructionInfo() },
            { OpCodes.Or64, new LFAssemblyInstructionInfo() },
            { OpCodes.XOr8, new LFAssemblyInstructionInfo() },
            { OpCodes.XOr16, new LFAssemblyInstructionInfo() },
            { OpCodes.XOr32, new LFAssemblyInstructionInfo() },
            { OpCodes.XOr64, new LFAssemblyInstructionInfo() },
            { OpCodes.Not8, new LFAssemblyInstructionInfo() },
            { OpCodes.Not16, new LFAssemblyInstructionInfo() },
            { OpCodes.Not32, new LFAssemblyInstructionInfo() },
            { OpCodes.Not64, new LFAssemblyInstructionInfo() },
            { OpCodes.Shl8, new LFAssemblyInstructionInfo() },
            { OpCodes.Shl16, new LFAssemblyInstructionInfo() },
            { OpCodes.Shl32, new LFAssemblyInstructionInfo() },
            { OpCodes.Shl64, new LFAssemblyInstructionInfo() },
            { OpCodes.Shr8, new LFAssemblyInstructionInfo() },
            { OpCodes.Shr16, new LFAssemblyInstructionInfo() },
            { OpCodes.Shr32, new LFAssemblyInstructionInfo() },
            { OpCodes.Shr64, new LFAssemblyInstructionInfo() },
            { OpCodes.Jump, new LFAssemblyInstructionInfo() },
            { OpCodes.JumpRel, new LFAssemblyInstructionInfo() },
            { OpCodes.JumpZero8, new LFAssemblyInstructionInfo() },
            { OpCodes.JumpZero16, new LFAssemblyInstructionInfo() },
            { OpCodes.JumpZero32, new LFAssemblyInstructionInfo() },
            { OpCodes.JumpZero64, new LFAssemblyInstructionInfo() },
            { OpCodes.JumpNotZero8, new LFAssemblyInstructionInfo() },
            { OpCodes.JumpNotZero16, new LFAssemblyInstructionInfo() },
            { OpCodes.JumpNotZero32, new LFAssemblyInstructionInfo() },
            { OpCodes.JumpNotZero64, new LFAssemblyInstructionInfo() },
            { OpCodes.Call, new LFAssemblyInstructionInfo() },
            { OpCodes.CallRel, new LFAssemblyInstructionInfo() },
            { OpCodes.Return, new LFAssemblyInstructionInfo() },
            { OpCodes.Load8, new LFAssemblyInstructionInfo() },
            { OpCodes.Load16, new LFAssemblyInstructionInfo() },
            { OpCodes.Load32, new LFAssemblyInstructionInfo() },
            { OpCodes.Load64, new LFAssemblyInstructionInfo() },
            { OpCodes.LoadN, new LFAssemblyInstructionInfo(8) },
            { OpCodes.Store8, new LFAssemblyInstructionInfo() },
            { OpCodes.Store16, new LFAssemblyInstructionInfo() },
            { OpCodes.Store32, new LFAssemblyInstructionInfo() },
            { OpCodes.Store64, new LFAssemblyInstructionInfo() },
            { OpCodes.StoreN, new LFAssemblyInstructionInfo(8) },
            { OpCodes.Assign8, new LFAssemblyInstructionInfo() },
            { OpCodes.Assign16, new LFAssemblyInstructionInfo() },
            { OpCodes.Assign32, new LFAssemblyInstructionInfo() },
            { OpCodes.Assign64, new LFAssemblyInstructionInfo() },
            { OpCodes.AssignN, new LFAssemblyInstructionInfo(8) },
            { OpCodes.Alloc, new LFAssemblyInstructionInfo() },
            { OpCodes.Free, new LFAssemblyInstructionInfo() },
            { OpCodes.WriteBus8, new LFAssemblyInstructionInfo() },
            { OpCodes.WriteBus16, new LFAssemblyInstructionInfo() },
            { OpCodes.WriteBus32, new LFAssemblyInstructionInfo() },
            { OpCodes.WriteBus64, new LFAssemblyInstructionInfo() },
            { OpCodes.ReadBus8, new LFAssemblyInstructionInfo() },
            { OpCodes.ReadBus16, new LFAssemblyInstructionInfo() },
            { OpCodes.ReadBus32, new LFAssemblyInstructionInfo() },
            { OpCodes.ReadBus64, new LFAssemblyInstructionInfo() },
            { OpCodes.TestZero8, new LFAssemblyInstructionInfo() },
            { OpCodes.TestZero16, new LFAssemblyInstructionInfo() },
            { OpCodes.TestZero32, new LFAssemblyInstructionInfo() },
            { OpCodes.TestZero64, new LFAssemblyInstructionInfo() },
            { OpCodes.TestEq8, new LFAssemblyInstructionInfo() },
            { OpCodes.TestEq16, new LFAssemblyInstructionInfo() },
            { OpCodes.TestEq32, new LFAssemblyInstructionInfo() },
            { OpCodes.TestEq64, new LFAssemblyInstructionInfo() },
            { OpCodes.TestNEq8, new LFAssemblyInstructionInfo() },
            { OpCodes.TestNEq16, new LFAssemblyInstructionInfo() },
            { OpCodes.TestNEq32, new LFAssemblyInstructionInfo() },
            { OpCodes.TestNEq64, new LFAssemblyInstructionInfo() },
            { OpCodes.TestLT8, new LFAssemblyInstructionInfo() },
            { OpCodes.TestLT16, new LFAssemblyInstructionInfo() },
            { OpCodes.TestLT32, new LFAssemblyInstructionInfo() },
            { OpCodes.TestLT64, new LFAssemblyInstructionInfo() },
            { OpCodes.TestGT8, new LFAssemblyInstructionInfo() },
            { OpCodes.TestGT16, new LFAssemblyInstructionInfo() },
            { OpCodes.TestGT32, new LFAssemblyInstructionInfo() },
            { OpCodes.TestGT64, new LFAssemblyInstructionInfo() },
            { OpCodes.TestLE8, new LFAssemblyInstructionInfo() },
            { OpCodes.TestLE16, new LFAssemblyInstructionInfo() },
            { OpCodes.TestLE32, new LFAssemblyInstructionInfo() },
            { OpCodes.TestLE64, new LFAssemblyInstructionInfo() },
            { OpCodes.TestGE8, new LFAssemblyInstructionInfo() },
            { OpCodes.TestGE16, new LFAssemblyInstructionInfo() },
            { OpCodes.TestGE32, new LFAssemblyInstructionInfo() },
            { OpCodes.TestGE64, new LFAssemblyInstructionInfo() },
            { OpCodes.Error, new LFAssemblyInstructionInfo() },
        };

    private readonly int[] m_ArgumentSizes;

    public LFAssemblyInstructionInfo(params int[] argSizes)
    {
        m_ArgumentSizes = argSizes;
    }

    public int ArgumentCount => m_ArgumentSizes.Length;

    public static int GetArgumentCount(OpCodes op)
    {
        return s_InstructionInfos[op].ArgumentCount;
    }

    public static int GetArgumentSize(OpCodes op, int argIndex)
    {
        return s_InstructionInfos[op].GetArgumentSize(argIndex);
    }

    public int GetArgumentSize(int i)
    {
        if (i < 0 || i >= m_ArgumentSizes.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(i));
        }

        return m_ArgumentSizes[i];
    }
}