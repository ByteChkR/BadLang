using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Bitwise;

namespace LF.Compiler.C.Operators.Bitwise;

public class LFCOrOperator : LFCPostValueOperator
{
    public LFCOrOperator() : base(12, "|") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCOrExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}