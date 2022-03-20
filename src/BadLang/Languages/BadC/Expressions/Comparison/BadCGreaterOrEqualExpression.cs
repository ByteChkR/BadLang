using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Comparison;

public class BadCGreaterOrEqualExpression : BadCBinaryExpression
{

    #region Public

    public BadCGreaterOrEqualExpression( BadCExpression left, BadCExpression right, SourceToken token ) :
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
            throw new ParseException( "Cannot assign to a comparison expression", SourceToken );
        }

        AssemblySymbol trueLabel = context.Writer.GetUniqueName( "OP_LESS_THAN_TRUE" );
        AssemblySymbol endLabel = context.Writer.GetUniqueName( "OP_LESS_THAN_END" );

        BadCType baseTypeHint = context.CheckTypes( SourceToken, Left, Right );

        context.AddSymbol( context.Writer, SourceToken );
        Left.Emit( context, baseTypeHint, false );
        Right.Emit( context, baseTypeHint, false );

        context.Writer.BranchGreaterOrEqual( baseTypeHint, trueLabel );
        context.Writer.Push( typeHint, 0 ); //Not zero, so push 0
        context.Writer.Jump( endLabel );
        context.Writer.Label( trueLabel, AssemblyElementVisibility.Local );
        context.Writer.Push( typeHint, 1 );                                //zero, so push 1
        context.Writer.Label( endLabel, AssemblyElementVisibility.Local ); //End Label
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCGreaterOrEqualExpression(
                                                Left.ResolveTemplateTypes( templateContext ),
                                                Right.ResolveTemplateTypes( templateContext ),
                                                SourceToken
                                               );
    }

    #endregion

}
