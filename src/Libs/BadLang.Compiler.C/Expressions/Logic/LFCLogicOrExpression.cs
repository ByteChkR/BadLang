using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Logic;

public class LFCLogicOrExpression : LFCBinaryExpression
{
    public LFCLogicOrExpression(LFCExpression left, LFCExpression right) : base(left, right) { }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        string rightLabel = input.Section.MakeUniqueLabel("__LFC__~LOGIC_OR~__RIGHT__");
        string endLabel = input.Section.MakeUniqueLabel("__LFC__~LOGIC_OR~__END__");
        string falseLabel = input.Section.MakeUniqueLabel("__LFC__~LOGIC_OR~__FALSE__");

        LFCExpressionCompileResult leftResult = Left.Compile(input.CreateInput(isLValue: false));

        input.Section.Emit(Position, OpCodes.Push64, rightLabel);
        input.Section.JumpIfZero(leftResult.ReturnType.GetSize(), Position);
        input.Section.Push((input.TypeHint ?? LFCBaseType.GetType("i8")).GetSize(), 1, Position);
        input.Section.Emit(Position, OpCodes.Push64, endLabel);
        input.Section.Emit(Position, OpCodes.Jump);

        input.Section.CreateLabel(rightLabel, Right.Position);
        LFCExpressionCompileResult rightResult = Right.Compile(input.CreateInput(isLValue: false));
        input.Section.Emit(Position, OpCodes.Push64, falseLabel);
        input.Section.JumpIfZero(leftResult.ReturnType.GetSize(), Position);
        input.Section.Push((input.TypeHint ?? LFCBaseType.GetType("i8")).GetSize(), 1, Position);
        input.Section.Emit(Position, OpCodes.Push64, endLabel);
        input.Section.Emit(Position, OpCodes.Jump);
        input.Section.CreateLabel(falseLabel, Position);
        input.Section.Push((input.TypeHint ?? LFCBaseType.GetType("i8")).GetSize(), 0, Position);

        input.Section.CreateLabel(endLabel, Position);

        if (leftResult.ReturnType != rightResult.ReturnType)
        {
            throw new LFCParserException("Logic OR expression must have the same return type", Position);
        }

        return input.CreateResult(input.TypeHint ?? LFCBaseType.GetType("i8"), Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCLogicOrExpression(rebuilder(Left), rebuilder(Right));
    }
}