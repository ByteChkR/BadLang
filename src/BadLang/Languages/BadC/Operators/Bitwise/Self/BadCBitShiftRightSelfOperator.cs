using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Bitwise.Self;

using BadVM.Shared;

namespace BadC.Operators.Bitwise.Self;

public class BadCBitShiftRightSelfOperator : BadCBinaryOperator
{

    #region Public

    public BadCBitShiftRightSelfOperator() : base( 16, ">>=" )
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

        return new BadCBitShiftRightSelfExpression( left, right, token );
    }

    #endregion

}
