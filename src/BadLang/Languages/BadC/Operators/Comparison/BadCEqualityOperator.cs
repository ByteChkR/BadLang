using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Comparison;

using BadVM.Shared;

namespace BadC.Operators.Comparison;

public class BadCEqualityOperator : BadCBinaryOperator
{

    #region Public

    public BadCEqualityOperator() : base( 10, "==" )
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

        return new BadCEqualityExpression( left, right, token );
    }

    #endregion

}
