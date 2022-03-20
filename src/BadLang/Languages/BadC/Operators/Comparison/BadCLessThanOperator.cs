using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Comparison;

using BadVM.Shared;

namespace BadC.Operators.Comparison;

public class BadCLessThanOperator : BadCBinaryOperator
{

    #region Public

    public BadCLessThanOperator() : base( 9, "<" )
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

        return new BadCLessThanExpression( left, right, token );
    }

    #endregion

}
