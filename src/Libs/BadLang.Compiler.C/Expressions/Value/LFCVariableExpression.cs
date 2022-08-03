using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Value;

public class LFCVariableExpression : LFCExpression
{
    private readonly LFCType? m_TypeHint;
    public readonly string Name;

    public LFCVariableExpression(string name, LFSourcePosition position, LFCType? typeHint = null) : base(position)
    {
        Name = name;
        m_TypeHint = typeHint;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return this;
    }

    protected virtual LFCType? GetTypeHint(LFCExpressionCompileInput input)
    {
        return m_TypeHint;
    }

    private LFCType LoadAddress(LFCExpressionCompileInput input)
    {
        if (input.Scope.HasVariable(Name))
        {
            input.Scope.LoadAddress(input.Section, Name, Position);

            return LFCPointerType.Create(input.Scope.GetVariableType(Name, Position));
        }

        throw new LFCParserException($"Variable '{Name}' is not defined.", Position);
    }


    private LFCType LoadValue(LFCExpressionCompileInput input)
    {
        if (input.Scope.HasVariable(Name))
        {
            input.Scope.LoadValue(input.Section, Name, Position);

            return input.Scope.GetVariableType(Name, Position);
        }

        if (input.HasFunction(Name))
        {
            return input.LoadFunction(Name, Position);
        }

        LFCType? type = GetTypeHint(input);
        if (type == null)
        {
            throw new LFCParserException($"Variable '{Name}' is not defined.", Position);
        }

        switch (type.GetSize())
        {
            case 1:
                input.Section.Emit(Position, OpCodes.Push8, Name);

                break;
            case 2:
                input.Section.Emit(Position, OpCodes.Push16, Name);

                break;
            case 4:
                input.Section.Emit(Position, OpCodes.Push32, Name);

                break;
            case 8:
                input.Section.Emit(Position, OpCodes.Push64, Name);

                break;
            default:
                throw new LFCParserException($"Variable '{Name}' has abnormal type size.", Position);
        }

        return type;
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        return input.CreateResult(input.IsLValue ? LoadAddress(input) : LoadValue(input), Position);
    }
}