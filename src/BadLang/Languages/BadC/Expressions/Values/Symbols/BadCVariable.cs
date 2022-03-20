using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Values.Symbols;

public class BadCVariable : BadCExpression
{

    public readonly AssemblySymbol Name;

    #region Public

    public BadCVariable( AssemblySymbol name, SourceToken token ) : base( false, token )
    {
        Name = name;
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval )
    {
        BadCVariableDeclaration decl = context.GetNamedVar( Name );

        if ( baseTypeHint == BadCType.GetPrimitive( BadCPrimitiveTypes.Void ) )
        {
            return;
        }

        if ( decl.Type != baseTypeHint )
        {
            throw new TypeMismatchException( decl.Type, baseTypeHint, SourceToken );
        }

        context.AddSymbol( context.Writer, SourceToken );

        if ( context.Function.TryGetStackFrameOffset( Name, out int off ) )
        {
            context.Writer.Emit( OpCode.PushSF, Array.Empty < byte >() );
            context.Writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )off ) );
            context.Writer.Emit( OpCode.AddI64, Array.Empty < byte >() );
        }
        else
        {
            AssemblyElement elem = context.Writer.AssemblyWriter.GetElement( Name );

            context.Writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) );
            context.Writer.AddPatchSite( Name, context.Writer.CurrentSize - sizeof( long ), sizeof( long ) );
        }

        if ( !isLval )
        {
            context.Writer.Load( decl.Type );
        }
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        BadCType t = context.GetNamedVar( Name ).Type;

        if ( t.PrimitiveType != null || t.IsPrimitivePointer )
        {
            return t;
        }

        if ( t.IsPointer )
        {
            return context.Writer.AssemblyWriter.CompilationData.GetData < BadCElementExporter >().
                           GetType( t.TypeName )?.
                           GetPointerType( t.PointerLevel );
        }

        return context.Writer.AssemblyWriter.CompilationData.GetData < BadCElementExporter >().
                       GetType( t.TypeName );
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return this;
    }

    public override string ToString()
    {
        return Name.ToString();
    }

    #endregion

}
