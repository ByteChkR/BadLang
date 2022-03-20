using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;

namespace BadC.Expressions.Type;

public class BadCSizeOfExpression : BadCExpression
{

    public BadCType Type { get; }

    #region Public

    public BadCSizeOfExpression( SourceToken sourceToken, BadCType type ) : base( false, sourceToken )
    {
        Type = type;
    }

    public override void Emit( BadCEmitContext context, BadCType baseTypeHint, bool isLval )
    {
        if ( baseTypeHint.IsPointer ||
             baseTypeHint.PrimitiveType == null ||
             baseTypeHint.PrimitiveType == BadCPrimitiveTypes.Void )
        {
            throw new ParseException(
                                     "Expected Primitive Number Type as type hint for 'sizeof' function",
                                     SourceToken
                                    );
        }

        context.AddSymbol( context.Writer, SourceToken );

        context.Writer.Push( baseTypeHint, Type.Size );
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCSizeOfExpression(
                                        SourceToken,
                                        templateContext.ResolveType( Type ).ResolveTemplate( templateContext )
                                       );
    }

    #endregion

}
