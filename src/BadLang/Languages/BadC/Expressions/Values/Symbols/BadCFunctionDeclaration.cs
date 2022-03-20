using BadC.Functions;
using BadC.Templates;
using BadC.Types.Primitives;

using BadVM.Shared;

namespace BadC.Expressions.Values.Symbols;

public class BadCFunctionDeclaration : BadCVariableDeclaration
{

    public FunctionInfo Signature { get; }

    #region Public

    public BadCFunctionDeclaration( FunctionInfo info, SourceToken token ) : base(
         info.Symbol,
         new BadCFunctionType( info.ReturnType, info.ParameterSize.Select( x => x.Type ).ToArray() ),
         token
        )
    {
        Signature = info;
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return this;
    }

    #endregion

}
