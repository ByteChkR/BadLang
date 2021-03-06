using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Bitwise;

using BadVM.Shared;

namespace BadC.Operators.Bitwise;

public class BadCShiftRightOperator : BadCBinaryOperator
{

    #region Public

    public BadCShiftRightOperator() : base( 7, ">>" )
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

        return new BadCShiftRightExpression( left, right, token );
    }

    #endregion

}
