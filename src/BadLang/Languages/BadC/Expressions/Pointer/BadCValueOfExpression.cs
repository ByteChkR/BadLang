using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;

namespace BadC.Expressions.Pointer;

public class BadCValueOfExpression : BadCExpression
{

    public BadCExpression Expression { get; }

    #region Public

    public BadCValueOfExpression( BadCExpression expression, SourceToken token ) : base( false, token )
    {
        Expression = expression;
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval )
    {
        context.AddSymbol( context.Writer, SourceToken );
        Expression.Emit( context, baseTypeHint.GetPointerType(), false );

        if ( isLval )
        {
            return;
        }

        BadCType? fixedType = Expression.GetFixedType( context );

        if ( fixedType != null &&
             fixedType.TryGetBaseType( out BadCType baseType ) &&
             baseType == BadCType.GetPrimitive( BadCPrimitiveTypes.Void ) )
        {
            throw new ParseException( "Cannot dereference void pointer", SourceToken );
        }

        context.Writer.Load( baseTypeHint );
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        BadCType? type = Expression.GetFixedType( context );

        if ( type == null )
        {
            return null;
        }

        return type.TryGetBaseType( out BadCType baseType ) ? baseType : null;
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCValueOfExpression(
                                         Expression.ResolveTemplateTypes( templateContext ),
                                         SourceToken
                                        );
    }

    #endregion

}
