using BadC.Templates;
using BadC.Types;

using BadVM.Shared;

namespace BadC.Expressions;

public abstract class BadCExpression
{

    public readonly bool IsConstant;
    public readonly SourceToken SourceToken;

    #region Public

    public abstract void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval );

    public abstract BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext );

    public virtual BadCType? GetFixedType( BadCEmitContext context )
    {
        return null;
    }

    #endregion

    #region Protected

    protected BadCExpression( bool isConstant, SourceToken sourceToken )
    {
        IsConstant = isConstant;
        SourceToken = sourceToken;
    }

    #endregion

}
