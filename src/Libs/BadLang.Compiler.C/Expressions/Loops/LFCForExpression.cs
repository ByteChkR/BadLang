using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Loops;

public class LFCForExpression : LFCExpression
{
    public readonly LFCExpression? Condition;
    public readonly LFCExpression? Increment;
    public readonly LFCExpression? Initializer;
    private readonly LFCExpression[] m_Body;

    public LFCForExpression(LFSourcePosition position, LFCExpression? initializer, LFCExpression? condition, LFCExpression? increment, LFCExpression[] body) : base(position)
    {
        Initializer = initializer;
        Condition = condition;
        Increment = increment;
        m_Body = body;
    }

    public IEnumerable<LFCExpression> Body => m_Body;

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        string forEndLabel = input.Section.MakeUniqueLabel("__LFC__~FOR~__END__");
        string forTopLabel = input.Section.MakeUniqueLabel("__LFC__~FOR~__TOP__");

        LFCChildScope forScope = input.Scope.CreateChildScope(Position);
        Initializer?.Compile(input.CreateInput(scope:forScope, isLValue: false, typeHint: null)).DiscardResult();

        //Label => TOP
        input.Section.CreateLabel(forTopLabel, Position);

        //Condition
        LFCType conditionType;
        if (Condition != null)
        {
            conditionType = Condition.Compile(
                    new LFCExpressionCompileInput(input.Result, input.Section, forScope, false)
                )
                .ReturnType;
        }
        else
        {
            input.Section.Emit(Position, OpCodes.Push8, 1);
            conditionType = LFCBaseType.GetType("i8");
        }

        //JUMPIFTRUE => END
        input.Section.Emit(Position, OpCodes.Push64, forEndLabel);
        input.Section.JumpIfZero(conditionType.GetSize(), Position);

        //Body
        LFCChildScope scope = forScope.CreateChildScope(Position, (i, p) => Continue(forTopLabel, i, p), (i, p) => LFCControlFlow.Break(forEndLabel, i, p));
        foreach (LFCExpression body in m_Body)
        {
            body.Compile(
                    new LFCExpressionCompileInput(input.Result, input.Section, scope, false)
                )
                .DiscardResult();
        }

        scope.Release(input.Section);

        //Increment
        Increment?.Compile(input.CreateInput(scope:forScope, isLValue: false, typeHint: null)).DiscardResult();

        //JUMP => TOP
        input.Section.Emit(Position, OpCodes.Push64, forTopLabel);
        input.Section.Emit(Position, OpCodes.Jump);
        input.Section.CreateLabel(forEndLabel, Position);
        forScope.Release(input.Section);

        return input.CreateResult(LFCBaseType.GetType("void"), Position);
    }

    private LFCExpressionCompileResult Continue(string startLabel, LFCExpressionCompileInput input, LFSourcePosition position)
    {
        Increment?.Compile(input.CreateInput(isLValue: false, typeHint: null)).DiscardResult();
        LFCExpressionCompileResult ret = LFCControlFlow.Continue(startLabel, input, position);

        return ret;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCForExpression(
            Position,
            Initializer == null ? null : rebuilder(Initializer),
            Condition == null ? null : rebuilder(Condition),
            Increment == null ? null : rebuilder(Increment),
            m_Body.Select(rebuilder).ToArray()
        );
    }
}