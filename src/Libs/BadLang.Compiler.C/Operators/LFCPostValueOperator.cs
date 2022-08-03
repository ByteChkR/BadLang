using LF.Compiler.C.Expressions;

namespace LF.Compiler.C.Operators;

public abstract class LFCPostValueOperator : LFCOperator
{
    protected LFCPostValueOperator(int precedence, string symbol) : base(precedence, symbol) { }
    public abstract LFCExpression Parse(LFCPostValueOperatorParseInput input);
}