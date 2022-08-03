using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Bitwise;

namespace LF.Compiler.C.Operators.Bitwise;

public class LFCAndOperator : LFCPostValueOperator
{
    public LFCAndOperator() : base(10, "&") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCAndExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}