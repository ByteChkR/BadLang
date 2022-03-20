using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Access.Member;

using BadVM.Shared;

namespace BadC.Operators.Access;

public class BadCDirectMemberAccessOperator : BadCBinaryOperator
{

    #region Public

    public BadCDirectMemberAccessOperator() : base( 2, "." )
    {
    }

    public override BadCExpression Parse(
        BadCEmitContext context,
        BadCExpression left,
        SourceReader reader,
        BadCCodeParser parser,
        SourceToken token )
    {
        string right = reader.ParseWord().StringValue;

        return new BadCDirectMemberAccessExpression( left, right, token );
    }

    #endregion

}
