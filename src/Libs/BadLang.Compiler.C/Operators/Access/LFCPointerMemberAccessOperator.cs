using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Access;

namespace LF.Compiler.C.Operators.Access;

public class LFCPointerMemberAccessOperator : LFCPostValueOperator
{
    public LFCPointerMemberAccessOperator() : base(2, "->") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        string right = input.Reader.ReadWord();

        return new LFCMemberAccessExpression(input.Left.Position.Combine(input.Reader.CreatePosition()), input.Left, right, true);
    }
}