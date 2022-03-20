using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Bitwise;

using BadVM.Shared;

namespace BadC.Operators.Bitwise;

public class BadCBitAndOperator : BadCBinaryOperator
{

    #region Public

    public BadCBitAndOperator() : base( 11, "&" )
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

        return new BadCBitAndExpression( left, right, token );
    }

    #endregion

}
