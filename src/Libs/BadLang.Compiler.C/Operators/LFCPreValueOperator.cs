using LF.Compiler.C.Expressions;

namespace LF.Compiler.C.Operators;

public abstract class LFCPreValueOperator : LFCOperator
{
    protected LFCPreValueOperator(int precedence, string symbol) : base(precedence, symbol) { }
    public abstract LFCExpression Parse(LFCPreValueOperatorParseInput input);
}