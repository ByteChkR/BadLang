using System.Text;

using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Access;
using LF.Compiler.C.Expressions.ControlFlow;
using LF.Compiler.C.Expressions.Value;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C;

internal static class LFCCompilerFunctions
{
    
    
    public static LFCType NameOf(LFCExpressionCompileInput input, LFCType[] types, LFCExpression[] expression, LFSourcePosition position)
    {
        if (expression.Length != 0)
        {
            throw new LFCParserException("NameOf() takes no arguments", position);
        }

        if (types.Length != 1)
        {
            throw new LFCParserException("NameOf() takes one type argument", position);
        }

        LFReaderDataSection data = input.Result.GetOrCreateDataSection("__~TYPE_INFO~__");
        string vName = data.EmitData(Encoding.UTF8.GetBytes(types[0].Name.ToString()));
        string name = $"{data.Name}::{vName}";

        input.Section.AddImport(data.Name);

        return new LFCVariableExpression(name, position, LFCBaseType.GetType("i8*")).Compile(input.CreateInput(isLValue: false, typeHint: LFCBaseType.GetType("i8*"))).ReturnType;
    }
    
    
    
    public static LFCType SizeOf(LFCExpressionCompileInput input, LFCType[] types, LFCExpression[] expression, LFSourcePosition position)
    {
        if (expression.Length != 0)
        {
            throw new LFCParserException("SizeOf() takes no arguments", position);
        }

        if (types.Length != 1)
        {
            throw new LFCParserException("SizeOf() takes one type argument", position);
        }

        int size = types[0].GetSize();
        int retSize;
        if (input.TypeHint != null)
        {
            retSize = input.TypeHint.GetSize();
        }
        else
        {
            retSize = 8;
        }

        switch (retSize)
        {
            case 1:
                input.Section.Emit(position, OpCodes.Push8, (byte)size);

                break;
            case 2:
                input.Section.Emit(position, OpCodes.Push16, (ushort)size);

                break;
            case 4:
                input.Section.Emit(position, OpCodes.Push32, (uint)size);

                break;
            case 8:
                input.Section.Emit(position, OpCodes.Push64, (ulong)size);

                break;
            default:
                throw new LFCParserException("Type hint is invalid", position);
        }

        return input.TypeHint ?? LFCBaseType.GetType("i64");
    }

    public static bool IsNumberSize(int size)
    {
        return size == 1 || size == 2 || size == 4 || size == 8;
    }

    public static LFCExpressionCompileResult UnsafeCastNumber(LFCExpressionCompileInput input, LFCType target, LFCExpression expression, LFSourcePosition position)
    {
        if (!IsNumberSize(target.GetSize()))
        {
            throw new LFCParserException("Target type is invalid", position);
        }

        LFCExpressionCompileResult output = expression.Compile(input.CreateInput(isLValue: false));

        if (!IsNumberSize(output.ReturnType.GetSize()))
        {
            throw new LFCParserException("Source type is invalid", position);
        }

        return UnsafeCast(input, target, output, position);
    }


    public static LFCExpressionCompileResult UnsafeCast(LFCExpressionCompileInput input, LFCType target, LFCExpressionCompileResult output, LFSourcePosition position)
    {
        if (output.ReturnType != target)
        {
            int currentSize = output.ReturnType.GetSize();
            int targetSize = target.GetSize();
            int delta = targetSize - currentSize;
            if (delta < 0)
            {
                int padding = -delta;
                input.Section.Pop(padding, position);
            }
            else if (delta > 0)
            {
                int padding = delta;
                input.Section.Push(padding, position);
            }
        }

        return input.CreateResult(target, position);
    }

    public static LFCExpressionCompileResult UnsafeCast(LFCExpressionCompileInput input, LFCType target, LFCExpression expression, LFSourcePosition position)
    {
        LFCExpressionCompileResult output = expression.Compile(input.CreateInput(isLValue: false));

        return UnsafeCast(input, target, output, position);
    }

    public static LFCType UnsafeCast(LFCExpressionCompileInput input, LFCType[] types, LFCExpression[] arguments, LFSourcePosition position)
    {
        if (types.Length != 1)
        {
            throw new LFCParserException("UnsafeCast can only be used with one type argument", position);
        }

        LFCType target = types[0];
        LFCExpression argument = arguments[0];
        LFCExpressionCompileResult output = UnsafeCast(input, target, argument, position);
        if (output.ReturnType != target)
        {
            throw new LFCParserException("UnsafeCast failed", position);
        }

        return output.ReturnType;
    }

    public static LFCType ConstConstructorCall(LFCExpressionCompileInput input, LFCType[] types, LFCExpression[] arguments, LFSourcePosition position)
    {
        if (types.Length != 0)
        {
            throw new LFCParserException("ConstConstructorCall can only be used with no type arguments", position);
        }

        if (arguments.Length < 1)
        {
            throw new LFCParserException("ConstConstructorCall can only be used with at least one argument", position);
        }

        LFCExpression target = arguments[0];


        LFCExpression[] args = arguments.Skip(1).ToArray();


        LFCInvocationExpression invoc = new LFCInvocationExpression(
            target.Position,
            new LFCMemberAccessExpression(target.Position, target, ".ctor", true),
            args,
            Array.Empty<LFCTypeToken>()
        );
        invoc.Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false)).DiscardResult();

        return LFCBaseType.GetType("void");
    }
}