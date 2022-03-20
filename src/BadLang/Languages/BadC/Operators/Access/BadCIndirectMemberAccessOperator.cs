using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Access.Member;

using BadVM.Shared;

namespace BadC.Operators.Access;

public class BadCIndirectMemberAccessOperator : BadCBinaryOperator
{

    #region Public

    public BadCIndirectMemberAccessOperator() : base( 2, "->" )
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

        return new BadCIndirectMemberAccessExpression( left, right, token );
    }

    #endregion

}
