using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Logic;

using BadVM.Shared;

namespace BadC.Operators.Logic;

public class BadCLogicOrOperator : BadCBinaryOperator
{

    #region Public

    public BadCLogicOrOperator() : base( 15, "||" )
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

        return new BadCLogicOrExpression( left, right, token );
    }

    #endregion

}
