using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Utils;

using BadVM.Shared;

namespace BadC.Expressions.Values;

public class BadCNumber : BadCExpression
{

    private readonly decimal m_Value;

    #region Public

    public BadCNumber( decimal d, SourceToken token ) : base( true, token )
    {
        m_Value = d;
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

        context.AddSymbol( context.Writer, SourceToken );

        context.Writer.Push( baseTypeHint, m_Value );
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return this;
    }

    #endregion

}
