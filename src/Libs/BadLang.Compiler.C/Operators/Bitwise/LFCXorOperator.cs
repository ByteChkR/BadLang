using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Bitwise;

namespace LF.Compiler.C.Operators.Bitwise;

public class LFCXorOperator : LFCPostValueOperator
{
    public LFCXorOperator() : base(11, "^") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCXorExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}