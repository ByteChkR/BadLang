using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Math.Atomic;

using BadVM.Shared;

namespace BadC.Operators.Math.Atomic;

public class BadCPreFixIncrementOperator : BadCUnaryOperator
{

    #region Public

    public BadCPreFixIncrementOperator() : base( 3, "++" )
    {
    }

    public override BadCExpression Parse(
        BadCEmitContext context,
        SourceReader reader,
        BadCCodeParser parser,
        SourceToken token )
    {
        BadCExpression left = parser.ParseExpression( reader, context, null, Precedence );

        return new BadCPreFixIncrementExpression( left, token );
    }

    #endregion

}
