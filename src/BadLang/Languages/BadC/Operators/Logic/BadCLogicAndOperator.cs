using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Logic;

using BadVM.Shared;

namespace BadC.Operators.Logic;

public class BadCLogicAndOperator : BadCBinaryOperator
{

    #region Public

    public BadCLogicAndOperator() : base( 14, "&&" )
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

        return new BadCLogicAndExpression( left, right, token );
    }

    #endregion

}
