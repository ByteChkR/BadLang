using LF.Compiler.DataObjects.Code.Instruction;

namespace LF.Compiler.Reader;

public static class LFReaderCodeSectionExtensions
{
    public static void Load(this LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        switch (size)
        {
            case 1:
                section.Emit(sourcePosition, OpCodes.Load8);

                break;
            case 2:
                section.Emit(sourcePosition, OpCodes.Load16);

                break;
            case 4:
                section.Emit(sourcePosition, OpCodes.Load32);

                break;
            case 8:
                section.Emit(sourcePosition, OpCodes.Load64);

                break;
            default:
                section.Emit(sourcePosition, OpCodes.LoadN, (ulong)size);

                break;
        }
    }


    public static void Store(this LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        switch (size)
        {
            case 1:
                section.Emit(sourcePosition, OpCodes.Store8);

                break;
            case 2:
                section.Emit(sourcePosition, OpCodes.Store16);

                break;
            case 4:
                section.Emit(sourcePosition, OpCodes.Store32);

                break;
            case 8:
                section.Emit(sourcePosition, OpCodes.Store64);

                break;
            default:
                section.Emit(sourcePosition, OpCodes.StoreN, (ulong)size);

                break;
        }
    }

    public static void Assign(this LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        switch (size)
        {
            case 1:
                section.Emit(sourcePosition, OpCodes.Assign8);

                break;
            case 2:
                section.Emit(sourcePosition, OpCodes.Assign16);

                break;
            case 4:
                section.Emit(sourcePosition, OpCodes.Assign32);

                break;
            case 8:
                section.Emit(sourcePosition, OpCodes.Assign64);

                break;
            default:
                section.Emit(sourcePosition, OpCodes.AssignN, (ulong)size);

                break;
        }
    }

    private static void PushOptimized(LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        int current = 0;
        while (current < size)
        {
            int left = size - current;
            if (left >= 8)
            {
                section.Emit(sourcePosition, OpCodes.Push64, 0);
                current += 8;
            }
            else if (left >= 4)
            {
                section.Emit(sourcePosition, OpCodes.Push32, 0);
                current += 4;
            }
            else if (left >= 2)
            {
                section.Emit(sourcePosition, OpCodes.Push16, 0);
                current += 2;
            }
            else
            {
                section.Emit(sourcePosition, OpCodes.Push8, 0);
                current += 1;
            }
        }
    }

    private static void PushNotOptimized(LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        for (int i = 0; i < size; i++)
        {
            section.Emit(sourcePosition, OpCodes.Push8, 0);
        }
    }

    public static void JumpIfZero(this LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        Emit(OpCodes.JumpZero8, section, size, sourcePosition);
    }

    public static void JumpIfNotZero(this LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        Emit(OpCodes.JumpNotZero8, section, size, sourcePosition);
    }

    private static void Emit(OpCodes op, LFReaderCodeSection section, int size, LFSourcePosition position, params LFAssemblyInstructionArgument[] args)
    {
        switch (size)
        {
            case 1:
                section.Emit(position, op, args);

                break;
            case 2:
                section.Emit(position, op + 1, args);

                break;
            case 4:
                section.Emit(position, op + 2, args);

                break;
            case 8:
                section.Emit(position, op + 3, args);

                break;
            default:
                throw new Exception($"Invalid size for {op} at {position}");
        }
    }

    public static void Add(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.Add8, section, size, position);
    }

    public static void Subtract(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.Sub8, section, size, position);
    }

    public static void Multiply(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.Mul8, section, size, position);
    }

    public static void Divide(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.Div8, section, size, position);
    }

    public static void Modulo(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.Mod8, section, size, position);
    }

    public static void BitwiseAnd(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.And8, section, size, position);
    }

    public static void BitwiseOr(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.Or8, section, size, position);
    }

    public static void BitwiseXor(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.XOr8, section, size, position);
    }

    public static void ShiftLeft(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.Shl8, section, size, position);
    }

    public static void ShiftRight(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.Shr8, section, size, position);
    }


    public static void Push(this LFReaderCodeSection section, int size, ulong value, LFSourcePosition position)
    {
        Emit(OpCodes.Push8, section, size, position, value);
    }

    public static void TestEq(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.TestEq8, section, size, position);
    }

    public static void TestNEq(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.TestNEq8, section, size, position);
    }

    public static void Dup(this LFReaderCodeSection section, int size, LFSourcePosition position)
    {
        Emit(OpCodes.Dup8, section, size, position);
    }

    public static void Push(this LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        if (LFCompilerOptimizationSettings.Instance.OptimizePush)
        {
            PushOptimized(section, size, sourcePosition);
        }
        else
        {
            PushNotOptimized(section, size, sourcePosition);
        }
    }

    private static void PopOptimized(LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        int current = 0;
        while (current < size)
        {
            int left = size - current;
            if (left >= 8)
            {
                section.Emit(sourcePosition, OpCodes.Pop64);
                current += 8;
            }
            else if (left >= 4)
            {
                section.Emit(sourcePosition, OpCodes.Pop32);
                current += 4;
            }
            else if (left >= 2)
            {
                section.Emit(sourcePosition, OpCodes.Pop16);
                current += 2;
            }
            else
            {
                section.Emit(sourcePosition, OpCodes.Pop8);
                current += 1;
            }
        }
    }

    private static void PopNotOptimized(LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        for (int i = 0; i < size; i++)
        {
            section.Emit(sourcePosition, OpCodes.Pop8);
        }
    }

    public static void Pop(this LFReaderCodeSection section, int size, LFSourcePosition sourcePosition)
    {
        if (LFCompilerOptimizationSettings.Instance.OptimizePop)
        {
            PopOptimized(section, size, sourcePosition);
        }
        else
        {
            PopNotOptimized(section, size, sourcePosition);
        }
    }
}