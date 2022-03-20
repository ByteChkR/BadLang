using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Comparison;

using BadVM.Shared;

namespace BadC.Operators.Comparison;

public class BadCGreaterOrEqualOperator : BadCBinaryOperator
{

    #region Public

    public BadCGreaterOrEqualOperator() : base( 9, ">=" )
    {
    }

    public override BadCExpression Parse(
        BadCEmitContext context,
        BadCExpression left,
        SourceReader reader,
        BadCCodeParser parser,
        SourceToken token )
    {
        BadCExpression right = parser.ParseExpression( reader, context, null, Precedence );

        return new BadCGreaterOrEqualExpression( left, right, token );
    }

    #endregion

}
