using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Functions;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;

using BadVM.Shared;

namespace BadC.Expressions.Flow;

public class BadCReturnExpression : BadCExpression
{

    public BadCExpression? Expression { get; }

    #region Public

    public BadCReturnExpression( BadCExpression? expression, SourceToken token ) : base( false, token )
    {
        Expression = expression;
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval )
    {
        context.AddSymbol( context.Writer, SourceToken );

        if ( Expression != null )
        {
            if ( context.Function.ReturnType == BadCType.GetPrimitive( BadCPrimitiveTypes.Void ) )
            {
                throw new ParseException( "Cannot return a value from a void function", SourceToken );
            }

            Expression.Emit( context, context.Function.ReturnType, false );
        }

        context.Function.WriteEpilouge( context.Writer );
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        return Expression == null
                   ? BadCType.GetPrimitive( BadCPrimitiveTypes.Void )
                   : Expression.GetFixedType( context );
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCReturnExpression(
                                        Expression?.ResolveTemplateTypes( templateContext ),
                                        SourceToken
                                       );
    }

    #endregion

}
