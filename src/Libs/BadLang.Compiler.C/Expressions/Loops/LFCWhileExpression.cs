using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Loops;

public class LFCWhileExpression : LFCExpression
{
    public readonly LFCExpression Condition;
    private readonly LFCExpression[] m_Body;

    public LFCWhileExpression(LFSourcePosition position, LFCExpression condition, LFCExpression[] body) : base(position)
    {
        Condition = condition;
        m_Body = body;
    }

    public IEnumerable<LFCExpression> Body => m_Body;

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCWhileExpression(
            Position,
            rebuilder(Condition),
            m_Body.Select(rebuilder).ToArray()
        );
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        string whileEndLabel = input.Section.MakeUniqueLabel("__LFC__~WHILE~__END__");
        string whileTopLabel = input.Section.MakeUniqueLabel("__LFC__~WHILE~__TOP__");

        //Label => TOP
        input.Section.CreateLabel(whileTopLabel, Position);

        //Condition
        LFCExpressionCompileResult conditionResult = Condition.Compile(
            new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false)
        );

        //JUMPIFTRUE => END
        input.Section.Emit(Position, OpCodes.Push64, whileEndLabel);
        switch (conditionResult.ReturnType.GetSize())
        {
            case 1:
                input.Section.Emit(Position, OpCodes.JumpZero8);

                break;
            case 2:
                input.Section.Emit(Position, OpCodes.JumpZero16);

                break;
            case 4:
                input.Section.Emit(Position, OpCodes.JumpZero32);

                break;
            case 8:
                input.Section.Emit(Position, OpCodes.JumpZero64);

                break;
            default:
                throw new LFCParserException($"Invalid size {conditionResult.ReturnType.GetSize()}", Position);
        }

        //Body
        LFCChildScope scope = input.Scope.CreateChildScope(Position, (i, p) => LFCControlFlow. Continue(whileTopLabel, i, p), (i, p) => LFCControlFlow.Break(whileEndLabel, i, p));
        foreach (LFCExpression body in Body)
        {
            body.Compile(
                    new LFCExpressionCompileInput(input.Result, input.Section, scope, false)
                )
                .DiscardResult();
        }

        scope.Release(input.Section);

        //JUMP => TOP
        input.Section.Emit(Position, OpCodes.Push64, whileTopLabel);
        input.Section.Emit(Position, OpCodes.Jump);
        input.Section.CreateLabel(whileEndLabel, Position);

        return input.CreateResult(LFCBaseType.GetType("void"), Position);
    }

    
}

public static class LFCControlFlow
{
    public static LFCExpressionCompileResult Break(string endLabel, LFCExpressionCompileInput input, LFSourcePosition position)
    {
        if (input.Scope is not LFCChildScope scope)
        {
            throw new LFCParserException("Invalid scope", position);
        }
        
        scope.Release(input.Section);
        input.Section.Emit(position, OpCodes.Push64, endLabel);
        input.Section.Emit(position, OpCodes.Jump);

        return input.CreateResult(LFCBaseType.GetType("void"), position);
    }
    
    public static LFCExpressionCompileResult Continue(string startLabel, LFCExpressionCompileInput input, LFSourcePosition position)
    {
        if (input.Scope is not LFCChildScope scope)
        {
            throw new LFCParserException("Invalid scope", position);
        }
        
        scope.Release(input.Section);
        input.Section.Emit(position, OpCodes.Push64, startLabel);
        input.Section.Emit(position, OpCodes.Jump);
        return input.CreateResult(LFCBaseType.GetType("void"), position);
    }
}