using BadVM.Shared;

namespace BadC.Expressions.Access.Member;

public abstract class BadCMemberAccessExpression : BadCExpression
{

    public BadCExpression Left { get; }

    public string Right { get; }

    #region Protected

    protected BadCMemberAccessExpression( BadCExpression left, string right, SourceToken token ) : base( false, token )
    {
        Left = left;
        Right = right;
    }

    #endregion

}
