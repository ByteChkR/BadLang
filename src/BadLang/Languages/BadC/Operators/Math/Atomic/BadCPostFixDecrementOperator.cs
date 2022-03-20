using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Math.Atomic;

using BadVM.Shared;

namespace BadC.Operators.Math.Atomic;

public class BadCPostFixDecrementOperator : BadCBinaryOperator
{

    #region Public

    public BadCPostFixDecrementOperator() : base( 2, "--" )
    {
    }

    public override BadCExpression Parse(
        BadCEmitContext context,
        BadCExpression left,
        SourceReader reader,
        BadCCodeParser parser,
        SourceToken token )
    {
        {
            return new BadCPostFixDecrementExpression( left, token );
        }
    }

    #endregion

}
