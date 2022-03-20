using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Math;

using BadVM.Shared;

namespace BadC.Operators.Math;

public class BadCMulOperator : BadCBinaryOperator
{

    #region Public

    public BadCMulOperator() : base( 5, "*" )
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

        return new BadCMulExpression( left, right, token );
    }

    #endregion

}
