using BadC.Types;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadC.Utils;

public static class CodeSectionWriterExtensions
{

    #region Public

    public static void Add( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.AddI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.AddI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.AddI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.AddI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public static void And( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.AndI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.AndI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.AndI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.AndI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public static void BranchGreater( this CodeSectionWriter writer, BadCType type, AssemblySymbol symbol )
    {
        writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) ); //Jump to true label
        writer.AddPatchSite( symbol, writer.CurrentSize - sizeof( long ), sizeof( long ) );
        writer.BranchGreater( type );
    }

    public static void BranchGreaterOrEqual( this CodeSectionWriter writer, BadCType type, AssemblySymbol symbol )
    {
        writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) ); //Jump to true label
        writer.AddPatchSite( symbol, writer.CurrentSize - sizeof( long ), sizeof( long ) );
        writer.BranchGreaterOrEqual( type );
    }

    public static void BranchLess( this CodeSectionWriter writer, BadCType type, AssemblySymbol symbol )
    {
        writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) ); //Jump to true label
        writer.AddPatchSite( symbol, writer.CurrentSize - sizeof( long ), sizeof( long ) );
        writer.BranchLess( type );
    }

    public static void BranchLessOrEqual( this CodeSectionWriter writer, BadCType type, AssemblySymbol symbol )
    {
        writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) ); //Jump to true label
        writer.AddPatchSite( symbol, writer.CurrentSize - sizeof( long ), sizeof( long ) );
        writer.BranchLessOrEqual( type );
    }

    public static void BranchNotZero( this CodeSectionWriter writer, BadCType type, AssemblySymbol symbol )
    {
        writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) ); //Jump to true label
        writer.AddPatchSite( symbol, writer.CurrentSize - sizeof( long ), sizeof( long ) );
        writer.BranchNotZero( type );
    }

    public static void BranchZero( this CodeSectionWriter writer, BadCType type, AssemblySymbol symbol )
    {
        writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) ); //Jump to true label
        writer.AddPatchSite( symbol, writer.CurrentSize - sizeof( long ), sizeof( long ) );
        writer.BranchZero( type );
    }

    public static void Div( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.DivI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.DivI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.DivI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.DivI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public static void Jump( this CodeSectionWriter writer, AssemblySymbol symbol )
    {
        writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) ); //Jump to true label
        writer.AddPatchSite( symbol, writer.CurrentSize - sizeof( long ), sizeof( long ) );
        writer.Emit( OpCode.Jump, Array.Empty < byte >() );
    }

    public static void Load( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.LoadI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.LoadI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.LoadI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.LoadI64, Array.Empty < byte >() );

                break;

            case 0:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );

            default:
                writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )type.Size ) );
                writer.Emit( OpCode.LoadN, Array.Empty < byte >() );

                break;
        }
    }

    public static void Mod( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.ModI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.ModI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.ModI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.ModI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public static void Mul( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.MulI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.MulI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.MulI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.MulI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public static void Or( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.OrI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.OrI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.OrI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.OrI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public static void Pop( this CodeSectionWriter writer, int size )
    {
        while ( size > 0 )
        {
            if ( size >= 8 )
            {
                writer.Emit(
                            OpCode.Pop64,
                            Array.Empty < byte >()
                           );

                size -= 8;
            }
            else if ( size >= 4 )
            {
                writer.Emit(
                            OpCode.Pop32,
                            Array.Empty < byte >()
                           );

                size -= 4;
            }
            else if ( size >= 2 )
            {
                writer.Emit(
                            OpCode.Pop16,
                            Array.Empty < byte >()
                           );

                size -= 2;
            }
            else
            {
                writer.Emit(
                            OpCode.Pop8,
                            Array.Empty < byte >()
                           );

                size -= 1;
            }
        }
    }

    public static void Push( this CodeSectionWriter writer, int size )
    {
        while ( size > 0 )
        {
            if ( size >= 8 )
            {
                writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) );
                size -= 8;
            }
            else if ( size >= 4 )
            {
                writer.Emit( OpCode.PushI32, BitConverter.GetBytes( ( int )0 ) );
                size -= 4;
            }
            else if ( size >= 2 )
            {
                writer.Emit( OpCode.PushI16, BitConverter.GetBytes( ( short )0 ) );
                size -= 2;
            }
            else
            {
                writer.Emit( OpCode.PushI8, new[] { ( byte )0 } );
                size -= 1;
            }
        }
    }

    public static void Push( this CodeSectionWriter writer, BadCType type, long v )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.PushI8, new[] { ( byte )v } );

                break;

            case 2:
                writer.Emit( OpCode.PushI16, BitConverter.GetBytes( ( short )v ) );

                break;

            case 4:
                writer.Emit( OpCode.PushI32, BitConverter.GetBytes( ( int )v ) );

                break;

            case 8:
                writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )v ) );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public static void Shl( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.ShlI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.ShlI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.ShlI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.ShlI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public static void Shr( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.ShrI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.ShrI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.ShrI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.ShrI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public static void Store( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.StoreI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.StoreI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.StoreI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.StoreI64, Array.Empty < byte >() );

                break;

            case 0:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );

            default:
                writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )type.Size ) );
                writer.Emit( OpCode.StoreN, Array.Empty < byte >() );

                break;
        }
    }

    public static void Sub( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.SubI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.SubI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.SubI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.SubI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public static void XOr( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.XorI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.XorI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.XorI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.XorI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    #endregion

    #region Private

    private static void BranchGreater( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.BranchGreaterI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.BranchGreaterI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.BranchGreaterI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.BranchGreaterI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    private static void BranchGreaterOrEqual( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.BranchGreaterOrEqualI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.BranchGreaterOrEqualI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.BranchGreaterOrEqualI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.BranchGreaterOrEqualI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    private static void BranchLess( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.BranchLessI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.BranchLessI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.BranchLessI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.BranchLessI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    private static void BranchLessOrEqual( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.BranchLessOrEqualI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.BranchLessOrEqualI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.BranchLessOrEqualI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.BranchLessOrEqualI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    private static void BranchNotZero( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.BranchNotZeroI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.BranchNotZeroI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.BranchNotZeroI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.BranchNotZeroI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    private static void BranchZero( this CodeSectionWriter writer, BadCType type )
    {
        switch ( type.Size )
        {
            case 1:
                writer.Emit( OpCode.BranchZeroI8, Array.Empty < byte >() );

                break;

            case 2:
                writer.Emit( OpCode.BranchZeroI16, Array.Empty < byte >() );

                break;

            case 4:
                writer.Emit( OpCode.BranchZeroI32, Array.Empty < byte >() );

                break;

            case 8:
                writer.Emit( OpCode.BranchZeroI64, Array.Empty < byte >() );

                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    #endregion

}
