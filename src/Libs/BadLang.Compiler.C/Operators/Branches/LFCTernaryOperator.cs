using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Branches;

namespace LF.Compiler.C.Operators.Branches;

public class LFCTernaryOperator : LFCPostValueOperator
{
    public LFCTernaryOperator() : base(16, "?") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        input.Reader.SkipNonToken();
        LFCExpression trueExpr = input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence);
        input.Reader.SkipNonToken();
        input.Reader.Eat(':');
        input.Reader.SkipNonToken();
        LFCExpression falseExpr = input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence);

        return new LFCTernaryExpression(input.Left, trueExpr, falseExpr);
    }
}