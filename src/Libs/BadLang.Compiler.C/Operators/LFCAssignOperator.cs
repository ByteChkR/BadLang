using LF.Compiler.C.Expressions;

namespace LF.Compiler.C.Operators;

public class LFCAssignOperator : LFCPostValueOperator
{
    public LFCAssignOperator() : base(15, "=") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCAssignExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}