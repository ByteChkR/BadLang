using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Bitwise;

using BadVM.Shared;

namespace BadC.Operators.Bitwise;

public class BadCShiftLeftOperator : BadCBinaryOperator
{

    #region Public

    public BadCShiftLeftOperator() : base( 7, "<<" )
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

        return new BadCShiftLeftExpression( left, right, token );
    }

    #endregion

}
