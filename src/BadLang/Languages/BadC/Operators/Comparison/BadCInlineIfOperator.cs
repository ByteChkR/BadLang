using BadAssembler;
using BadC.Expressions;
using BadC.Expressions.Branch;
using BadVM.Shared;

namespace BadC.Operators.Comparison;

public class BadCInlineIfOperator : BadCBinaryOperator
{
    public BadCInlineIfOperator() : base(16, "?")
    {
    }

    public override BadCExpression Parse(BadCEmitContext context, BadCExpression left, SourceReader reader, BadCCodeParser parser,
        SourceToken token)
    {
        BadCExpression trueExpr = parser.ParseExpression(reader, context, null);
        reader.SkipWhitespaceAndNewlineAndComments("//");
        reader.Eat(":");
        reader.SkipWhitespaceAndNewlineAndComments("//");
        BadCExpression falseExpr = parser.ParseExpression(reader, context, null);
        return new BadCInlineIfExpression(token ,left, trueExpr, falseExpr);
        
    }
}