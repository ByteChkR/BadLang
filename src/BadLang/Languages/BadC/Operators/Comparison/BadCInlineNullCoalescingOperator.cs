using BadAssembler;
using BadC.Expressions;
using BadC.Expressions.Branch;
using BadVM.Shared;

namespace BadC.Operators.Comparison;

public class BadCInlineNullCoalescingOperator : BadCBinaryOperator
{
    public BadCInlineNullCoalescingOperator() : base(16, "??")
    {
    }

    public override BadCExpression Parse(BadCEmitContext context, BadCExpression left, SourceReader reader, BadCCodeParser parser,
        SourceToken token)
    {
        BadCExpression right = parser.ParseExpression(reader, context, null);
        return new BadCInlineNullCoalescingExpression(left, right, token);
    }
}