using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Bitwise;

using BadVM.Shared;

namespace BadC.Operators.Bitwise;

public class BadCBitOrOperator : BadCBinaryOperator
{

    #region Public

    public BadCBitOrOperator() : base( 13, "|" )
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

        return new BadCBitOrExpression( left, right, token );
    }

    #endregion

}
