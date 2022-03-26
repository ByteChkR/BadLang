using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadVM.Shared;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadC.Expressions.Type;

public class BadCCastExpression : BadCExpression
{

    private static Dictionary < int, Dictionary < int, Action < CodeSectionWriter > > > m_Converters =
        new()
        {
            {
                1, new Dictionary < int, Action < CodeSectionWriter > >
                   {
                       { 1, writer => { } },
                       { 2, From1To2 },
                       { 4, From1To4 },
                       { 8, From1To8 }
                   }
            },
            {
                2, new Dictionary < int, Action < CodeSectionWriter > >
                   {
                       { 1, From2To1 },
                       { 2, writer => { } },
                       { 4, From2To4 },
                       { 8, From2To8 }
                   }
            },
            {
                4, new Dictionary < int, Action < CodeSectionWriter > >
                   {
                       { 1, From4To1 },
                       { 2, From4To2 },
                       { 4, writer => { } },
                       { 8, From4To8 }
                   }
            },
            {
                8, new Dictionary < int, Action < CodeSectionWriter > >
                   {
                       { 1, From8To1 },
                       { 2, From8To2 },
                       { 4, From8To4 },
                       { 8, writer => { } },
                   }
            }
        };

    public BadCExpression Expression { get; }

    public BadCType TargetType { get; }

    #region Public

    public BadCCastExpression( SourceToken sourceToken, BadCExpression expression, BadCType targetType ) :
        base( false, sourceToken )
    {
        Expression = expression;
        TargetType = targetType;
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval )
    {
        BadCType? exprType = Expression.GetFixedType( context );

        context.AddSymbol( context.Writer, SourceToken );

        if (exprType != null && 
            (exprType.PrimitiveType is BadCPrimitiveTypes.F32 or BadCPrimitiveTypes.F64 || 
             TargetType.PrimitiveType is BadCPrimitiveTypes.F32 or BadCPrimitiveTypes.F64))
        {
            Expression.Emit( context, exprType, isLval );

            if (TargetType.PrimitiveType == exprType.PrimitiveType) return;
            
            if (exprType.PrimitiveType == BadCPrimitiveTypes.F64)
            {
                context.Writer.Emit(OpCode.F64ToI64, Array.Empty<byte>());
                if (TargetType.PrimitiveType != BadCPrimitiveTypes.I64)
                {
                    m_Converters[exprType.Size][TargetType.Size]( context.Writer );
                }
            }
            else if (exprType.PrimitiveType == BadCPrimitiveTypes.F32)
            {
                context.Writer.Emit(OpCode.F32ToI32, Array.Empty<byte>());
                if (TargetType.PrimitiveType != BadCPrimitiveTypes.I32)
                {
                    m_Converters[exprType.Size][TargetType.Size]( context.Writer );
                }
            }
            else
            {
                m_Converters[exprType.Size][TargetType.Size]( context.Writer );
                if (TargetType.PrimitiveType == BadCPrimitiveTypes.F32)
                {
                    context.Writer.Emit(OpCode.I32ToF32, Array.Empty<byte>());
                }
                else if (TargetType.PrimitiveType == BadCPrimitiveTypes.F64)
                {
                    context.Writer.Emit(OpCode.I64ToF64, Array.Empty<byte>());
                }
                else throw new Exception();
            }
            
            

            return;
        }



        if ( exprType == null || exprType.Size == TargetType.Size )
        {
            Expression.Emit( context, exprType ?? baseTypeHint, isLval );

            return;
        }

        Expression.Emit( context, exprType, isLval );

        m_Converters[exprType.Size][TargetType.Size]( context.Writer );
        if (TargetType.PrimitiveType == BadCPrimitiveTypes.F64)
        {
            context.Writer.Emit(OpCode.I64ToF64, Array.Empty<byte>());
            
        }
        else if(TargetType.PrimitiveType == BadCPrimitiveTypes.F32)
        {
            context.Writer.Emit(OpCode.I32ToF32, Array.Empty<byte>());
        }
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        return TargetType;
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        BadCType t = templateContext.ResolveType( TargetType );

        return new BadCCastExpression(
                                      SourceToken,
                                      Expression.ResolveTemplateTypes( templateContext ),
                                      t
                                     );
    }

    #endregion

    #region Private

    private static void From1To2( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.DupI8, Array.Empty < byte >() );
        writer.Emit( OpCode.PushI16, BitConverter.GetBytes( ( short )0xFF ) );
        writer.Emit( OpCode.AndI16, Array.Empty < byte >() );
    }

    private static void From1To4( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.DupI8, Array.Empty < byte >() );
        writer.Emit( OpCode.DupI16, Array.Empty < byte >() );
        writer.Emit( OpCode.PushI32, BitConverter.GetBytes( ( int )0xFF ) );
        writer.Emit( OpCode.AndI32, Array.Empty < byte >() );
    }

    private static void From1To8( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.DupI8, Array.Empty < byte >() );
        writer.Emit( OpCode.DupI16, Array.Empty < byte >() );
        writer.Emit( OpCode.DupI32, Array.Empty < byte >() );
        writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0xFF ) );
        writer.Emit( OpCode.AndI64, Array.Empty < byte >() );
    }

    private static void From2To1( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.SwapI8, Array.Empty < byte >() );
        writer.Emit( OpCode.Pop8, Array.Empty < byte >() );
    }

    private static void From2To4( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.DupI16, Array.Empty < byte >() );
        writer.Emit( OpCode.PushI32, BitConverter.GetBytes( ( int )0xFFFF ) );
        writer.Emit( OpCode.AndI32, Array.Empty < byte >() );
    }

    private static void From2To8( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.DupI16, Array.Empty < byte >() );
        writer.Emit( OpCode.DupI32, Array.Empty < byte >() );
        writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0xFFFF ) );
        writer.Emit( OpCode.AndI64, Array.Empty < byte >() );
    }

    private static void From4To1( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.SwapI16, Array.Empty < byte >() );
        writer.Emit( OpCode.Pop16, Array.Empty < byte >() );
        writer.Emit( OpCode.SwapI8, Array.Empty < byte >() );
        writer.Emit( OpCode.Pop8, Array.Empty < byte >() );
    }

    private static void From4To2( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.SwapI8, Array.Empty < byte >() );
        writer.Emit( OpCode.Pop8, Array.Empty < byte >() );
    }

    private static void From4To8( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.DupI32, Array.Empty < byte >() );
        writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0xFFFFFFFF ) );
        writer.Emit( OpCode.AndI64, Array.Empty < byte >() );
    }

    private static void From8To1( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.SwapI32, Array.Empty < byte >() );
        writer.Emit( OpCode.Pop32, Array.Empty < byte >() );
        writer.Emit( OpCode.SwapI16, Array.Empty < byte >() );
        writer.Emit( OpCode.Pop16, Array.Empty < byte >() );
        writer.Emit( OpCode.SwapI8, Array.Empty < byte >() );
        writer.Emit( OpCode.Pop8, Array.Empty < byte >() );
    }

    private static void From8To2( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.SwapI32, Array.Empty < byte >() );
        writer.Emit( OpCode.Pop32, Array.Empty < byte >() );
        writer.Emit( OpCode.SwapI16, Array.Empty < byte >() );
        writer.Emit( OpCode.Pop16, Array.Empty < byte >() );
    }

    private static void From8To4( CodeSectionWriter writer )
    {
        writer.Emit( OpCode.SwapI32, Array.Empty < byte >() );
        writer.Emit( OpCode.Pop32, Array.Empty < byte >() );
    }

    #endregion

}
