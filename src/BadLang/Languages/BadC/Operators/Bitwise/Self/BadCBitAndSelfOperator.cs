using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Bitwise.Self;

using BadVM.Shared;

namespace BadC.Operators.Bitwise.Self;

public class BadCBitAndSelfOperator : BadCBinaryOperator
{

    #region Public

    public BadCBitAndSelfOperator() : base( 16, "&=" )
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

        return new BadCBitAndSelfExpression( left, right, token );
    }

    #endregion

}
