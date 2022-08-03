using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Comparison;

namespace LF.Compiler.C.Operators.Comparison;

public class LFCGreaterThanOperator : LFCPostValueOperator
{
    public LFCGreaterThanOperator() : base(8, ">") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCGreaterThanExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}