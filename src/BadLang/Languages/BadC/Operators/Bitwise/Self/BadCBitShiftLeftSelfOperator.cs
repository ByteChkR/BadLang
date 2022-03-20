using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Bitwise.Self;

using BadVM.Shared;

namespace BadC.Operators.Bitwise.Self;

public class BadCBitShiftLeftSelfOperator : BadCBinaryOperator
{

    #region Public

    public BadCBitShiftLeftSelfOperator() : base( 16, "<<=" )
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

        return new BadCBitShiftLeftSelfExpression( left, right, token );
    }

    #endregion

}
