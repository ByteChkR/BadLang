using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Logic;

namespace LF.Compiler.C.Operators.Logic;

public class LFCLogicOrOperator : LFCPostValueOperator
{
    public LFCLogicOrOperator() : base(15, "||") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCLogicOrExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}