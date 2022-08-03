using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Branches;

public class LFCTernaryExpression : LFCExpression
{
    public readonly LFCExpression Condition;
    public readonly LFCExpression FalseExpression;
    public readonly LFCExpression TrueExpression;

    public LFCTernaryExpression(LFCExpression condition, LFCExpression trueExpression, LFCExpression falseExpression) : base(condition.Position.Combine(falseExpression.Position))
    {
        Condition = condition;
        TrueExpression = trueExpression;
        FalseExpression = falseExpression;
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        string labelFalse = input.Section.MakeUniqueLabel("__LFC__~TERNARY_FALSE~__");
        string labelEnd = input.Section.MakeUniqueLabel("__LFC__~TERNARY_END~__");

        LFCExpressionCompileResult conditionResult = Condition.Compile(input.CreateInput(isLValue: false, typeHint: null));

        input.Section.Emit(Position, OpCodes.Push64, labelFalse);
        input.Section.JumpIfZero(conditionResult.ReturnType.GetSize(), Position);

        LFCExpressionCompileResult trueResult = TrueExpression.Compile(input.CreateInput(isLValue: false));
        input.Section.Emit(Position, OpCodes.Push64, labelEnd);
        input.Section.Emit(Position, OpCodes.Jump);

        input.Section.CreateLabel(labelFalse, Position);
        LFCExpressionCompileResult falseResult = FalseExpression.Compile(input.CreateInput(isLValue: false));
        input.Section.CreateLabel(labelEnd, Position);

        if (trueResult.ReturnType != falseResult.ReturnType)
        {
            throw new LFCParserException("Ternary true/false expressions must return the same type", Position);
        }

        return input.CreateResult(trueResult.ReturnType, Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCTernaryExpression(rebuilder(Condition), rebuilder(TrueExpression), rebuilder(FalseExpression));
    }
}