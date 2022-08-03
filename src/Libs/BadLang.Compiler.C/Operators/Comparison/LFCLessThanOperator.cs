using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Comparison;

namespace LF.Compiler.C.Operators.Comparison;

public class LFCLessThanOperator : LFCPostValueOperator
{
    public LFCLessThanOperator() : base(8, "<") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCLessThanExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}