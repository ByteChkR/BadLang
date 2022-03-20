using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Expressions.Values.Symbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;

namespace BadC.Expressions.Math.Atomic;

public class BadCPreFixIncrementExpression : BadCExpression
{

    public BadCExpression Left { get; }

    #region Public

    public BadCPreFixIncrementExpression( BadCExpression left, SourceToken token ) : base( false, token )
    {
        Left = left;
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

        if ( Left is not BadCVariable var )
        {
            throw new ParseException( "Left side of assignment must be a variable", SourceToken );
        }

        BadCType baseTypeHint = context.GetNamedVar( var.Name ).Type;

        context.AddSymbol( context.Writer, SourceToken );

        context.Writer.Push( baseTypeHint, 1 );

        Left.Emit( context, baseTypeHint, false );

        context.Writer.Add( baseTypeHint );

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

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCPreFixIncrementExpression(
                                                 Left.ResolveTemplateTypes(
                                                                           templateContext
                                                                          ),
                                                 SourceToken
                                                );
    }

    #endregion

}
