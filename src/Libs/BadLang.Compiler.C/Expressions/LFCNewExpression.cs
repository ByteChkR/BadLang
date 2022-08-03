using LF.Compiler.C.Expressions.Access;
using LF.Compiler.C.Expressions.ControlFlow;
using LF.Compiler.C.Expressions.Value;
using LF.Compiler.C.Functions;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions;

public class LFCNewExpression : LFCExpression
{
    private readonly LFCExpression[] m_Parameters;
    public readonly LFCTypeToken Type;

    public LFCNewExpression(LFSourcePosition position, LFCTypeToken type, LFCExpression[] parameters) : base(position)
    {
        Type = type;
        m_Parameters = parameters;
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCType type = input.GetType(Type, Position);


        if (!type.HasConstructor)
        {
            throw new LFCParserException("Type " + type.Name + " has no constructor", Position);
        }

        LFCType ctorType = type.GetPropertyType(LFCStructureType.GetFunctionName(Type.Name, ".ctor"), Position);
        LFCTypeDatabase db = LFCTypeDatabase.GetTypeSystem(input.Result);
        if (ctorType is not LFCFunctionType funcType)
        {
            throw new LFCParserException("Type " + type.Name + " has no constructor", Position);
        }

        if (funcType.SourceFunction == null)
        {
            throw new LFCParserException("Type " + type.Name + " has invalid constructor", Position);
        }

        input.Section.Emit(Position, OpCodes.Push32, (ulong)type.GetSize());
        input.Section.Emit(Position, OpCodes.Alloc);
        input.Section.Emit(Position, OpCodes.Dup64); //Duplicate pointer for the constructor call

        LFCInvocationExpression invoc = new LFCInvocationExpression(
            Position,
            new LFCMemberAccessExpression(
                Position,
                new LFCVariableExpression(type.Name.Name, Position),
                ".ctor",
                false
            ),
            m_Parameters,
            Array.Empty<LFCTypeToken>()
        );
        invoc.InvokeInstance(
                new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false),
                new LFCExpressionCompileResult(LFCPointerType.Create(type), input.Result, input.Section, input.Section.InstructionCount, false, Type.TypeArgs),
                db,
                funcType
            )
            .DiscardResult();

        //invoc.Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false)).DiscardResult();

        return input.CreateResult(LFCPointerType.Create(type), Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCNewExpression(Position, Type, m_Parameters.Select(rebuilder).ToArray());
    }
}