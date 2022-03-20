using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Comparison;

using BadVM.Shared;

namespace BadC.Operators.Comparison;

public class BadCLessOrEqualOperator : BadCBinaryOperator
{

    #region Public

    public BadCLessOrEqualOperator() : base( 9, "<=" )
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

        return new BadCLessOrEqualExpression( left, right, token );
    }

    #endregion

}
