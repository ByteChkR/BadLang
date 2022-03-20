using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;

namespace BadC.Expressions.Assignment;

public class BadCAssignmentExpression : BadCBinaryExpression
{

    #region Public

    public BadCAssignmentExpression( BadCExpression left, BadCExpression right, SourceToken token ) :
        base( left, right, token )
    {
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType typeHint,
        bool isLval )
    {
        if ( isLval )
        {
            throw new ParseException( "Assignment expressions cannot be lvalues", SourceToken );
        }

        BadCType? baseTypeHint = Left.GetFixedType( context );

        if ( baseTypeHint == null )
        {
            throw new ParseException( "Cannot determine type of left-hand side of assignment", SourceToken );
        }

        context.AddSymbol( context.Writer, SourceToken );

        if ( baseTypeHint is BadCFunctionType ft )
        {
            Right.Emit( context, ft.GetPointerType(), false );
        }
        else
        {
            Right.Emit( context, baseTypeHint, false );
        }

        Left.Emit( context, baseTypeHint, true );

        context.Writer.Store( baseTypeHint );

        if ( typeHint != BadCType.GetPrimitive( BadCPrimitiveTypes.Void ) )
        {
            Left.Emit(
                      context,
                      typeHint,
                      false
                     ); //Duplicate to return the value from the expression if we dont have void return
        }
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        return Left.GetFixedType( context ) ??
               throw new ParseException( "Cannot determine type of left-hand side of assignment", SourceToken );
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCAssignmentExpression(
                                            Left.ResolveTemplateTypes( templateContext ),
                                            Right.ResolveTemplateTypes( templateContext ),
                                            SourceToken
                                           );
    }

    #endregion

}
