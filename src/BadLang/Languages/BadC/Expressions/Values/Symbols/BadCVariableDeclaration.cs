using BadC.Templates;
using BadC.Types;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Values.Symbols;

public class BadCVariableDeclaration : BadCVariable
{

    public readonly BadCType Type;

    #region Public

    public BadCVariableDeclaration( AssemblySymbol name, BadCType type, SourceToken token ) : base( name, token )
    {
        Type = type;
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        if ( Type.IsResolved )
        {
            return this;
        }

        BadCType t = templateContext.ResolveType( Type ).ResolveTemplate( templateContext );

        return new BadCVariableDeclaration( Name, t, SourceToken );
    }

    public override string ToString()
    {
        return $"{Type} {Name.SymbolName}";
    }

    #endregion

}
