using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Logic;

namespace LF.Compiler.C.Operators.Logic;

public class LFCLogicAndOperator : LFCPostValueOperator
{
    public LFCLogicAndOperator() : base(14, "&&") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCLogicAndExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}