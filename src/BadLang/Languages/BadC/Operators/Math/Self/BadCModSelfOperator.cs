using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Math.Self;

using BadVM.Shared;

namespace BadC.Operators.Math.Self;

public class BadCModSelfOperator : BadCBinaryOperator
{

    #region Public

    public BadCModSelfOperator() : base( 16, "%=" )
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

        return new BadCModSelfExpression( left, right, token );
    }

    #endregion

}
