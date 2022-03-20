using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Math.Atomic;

using BadVM.Shared;

namespace BadC.Operators.Math.Atomic;

public class BadCPostFixIncrementOperator : BadCBinaryOperator
{

    #region Public

    public BadCPostFixIncrementOperator() : base( 2, "++" )
    {
    }

    public override BadCExpression Parse(
        BadCEmitContext context,
        BadCExpression left,
        SourceReader reader,
        BadCCodeParser parser,
        SourceToken token )
    {
        return new BadCPostFixIncrementExpression( left, token );
    }

    #endregion

}
