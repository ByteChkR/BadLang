using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Utils;

using BadVM.Shared;

namespace BadC.Expressions.Math;

public class BadCModExpression : BadCBinaryExpression
{

    #region Public

    public BadCModExpression( BadCExpression left, BadCExpression right, SourceToken token ) : base(
         left,
         right,
         token
        )
    {
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval )
    {
        if ( isLval )
        {
            throw new ParseException( "Cannot assign to a number", SourceToken );
        }

        baseTypeHint = context.CheckTypes( SourceToken, Left, Right, baseTypeHint )!;

        context.AddSymbol( context.Writer, SourceToken );
        Left.Emit( context, baseTypeHint, isLval );
        Right.Emit( context, baseTypeHint, isLval );

        context.Writer.Mod( baseTypeHint );
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCModExpression(
                                     Left.ResolveTemplateTypes( templateContext ),
                                     Right.ResolveTemplateTypes( templateContext ),
                                     SourceToken
                                    );
    }

    #endregion

}
