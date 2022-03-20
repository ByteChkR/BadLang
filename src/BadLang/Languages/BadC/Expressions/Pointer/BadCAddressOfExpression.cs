using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Expressions.Values.Symbols;
using BadC.Templates;
using BadC.Types;

using BadVM.Shared;

namespace BadC.Expressions.Pointer;

public class BadCAddressOfExpression : BadCExpression
{

    public BadCExpression Expression { get; }

    #region Public

    public BadCAddressOfExpression( BadCExpression expression, SourceToken token ) : base( false, token )
    {
        Expression = expression;
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval )
    {
        if ( Expression is not BadCVariable var )
        {
            throw new ParseException( "AddressOf Target must be a variable", SourceToken );
        }

        BadCType? vType = context.GetNamedVar( var.Name ).Type.GetPointerType();

        if ( vType == null )
        {
            throw new ParseException( "AddressOf Target must be a variable with a fixed type", SourceToken );
        }

        if ( baseTypeHint != vType )
        {
            throw new TypeMismatchException( vType, baseTypeHint, SourceToken );
        }

        context.AddSymbol( context.Writer, SourceToken );
        baseTypeHint = context.GetNamedVar( var.Name ).Type;
        Expression.Emit( context, baseTypeHint, true );
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        if ( Expression is BadCVariable var )
        {
            return context.GetNamedVar( var.Name ).Type.GetPointerType();
        }

        return null;
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCAddressOfExpression(
                                           Expression.ResolveTemplateTypes(
                                                                           templateContext
                                                                          ),
                                           SourceToken
                                          );
    }

    #endregion

}
