using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Bitwise;

using BadVM.Shared;

namespace BadC.Operators.Bitwise;

public class BadCBitXOrOperator : BadCBinaryOperator
{

    #region Public

    public BadCBitXOrOperator() : base( 12, "^" )
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

        return new BadCBitXOrExpression( left, right, token );
    }

    #endregion

}
