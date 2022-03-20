using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;

using BadVM.Shared;

namespace BadC.Expressions.Flow;

public class BadCHaltExpression : BadCExpression
{

    #region Public

    public BadCHaltExpression( SourceToken token ) : base( false, token )
    {
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval )
    {
        context.AddSymbol( context.Writer, SourceToken );
        context.Writer.Emit( OpCode.Halt, Array.Empty < byte >() );
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        return BadCType.GetPrimitive( BadCPrimitiveTypes.Void );
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return this;
    }

    #endregion

}
