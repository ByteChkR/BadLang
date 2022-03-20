using System.Runtime.InteropServices;

using BadAssembler.Exceptions;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadAssembler.AssemblerSyntax
{

    public class BadAssemblyInstructionParser
    {

        private static readonly List < BadAssemblyInstructionParser > s_InstructionParsers =
            new List < BadAssemblyInstructionParser >();

        private readonly Action < OpCode, SourceReader, CodeSectionWriter > m_Parser;

        public OpCode OpCode { get; }

        #region Public

        public static T ConvertNumber < T >( decimal value )
        {
            if ( value < 0 )
            {
                ulong
                    l = ( ulong )( long )value; //Fix Overflow exception when trying to directly cast a negative number to ulong

                return ( T )Convert.ChangeType( l, typeof( T ) );
            }

            return ( T )Convert.ChangeType( value, typeof( T ) );
        }

        public static byte[] GetBytes( object o )
        {
            switch ( o.GetType() )
            {
                case { } t when t == typeof( byte ):
                    return new byte[] { ( byte )o };

                case { } t when t == typeof( sbyte ):
                    return new byte[] { ( byte )( sbyte )o };

                case { } t when t == typeof( short ):
                    return BitConverter.GetBytes( ( short )o );

                case { } t when t == typeof( ushort ):
                    return BitConverter.GetBytes( ( ushort )o );

                case { } t when t == typeof( int ):
                    return BitConverter.GetBytes( ( int )o );

                case { } t when t == typeof( uint ):
                    return BitConverter.GetBytes( ( uint )o );

                case { } t when t == typeof( long ):
                    return BitConverter.GetBytes( ( long )o );

                case { } t when t == typeof( ulong ):
                    return BitConverter.GetBytes( ( ulong )o );

                case { } t when t == typeof( float ):
                    return BitConverter.GetBytes( ( float )o );

                case { } t when t == typeof( double ):
                    return BitConverter.GetBytes( ( double )o );
            }

            throw new ArgumentException( "Unsupported type" );
        }

        public static void ParseInstruction(
            OpCode op,
            SourceToken opToken,
            SourceReader reader,
            CodeSectionWriter writer )
        {
            BadAssemblyInstructionParser p = s_InstructionParsers.FirstOrDefault( x => x.OpCode == op );

            if ( p == null )
            {
                throw new ParseException( $"Unknown opcode {op}", opToken );
            }

            p.Parse( reader, writer );
        }

        public void Parse( SourceReader reader, CodeSectionWriter writer )
        {
            m_Parser( OpCode, reader, writer );
        }

        #endregion

        #region Private

        static BadAssemblyInstructionParser()
        {
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.Nop, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.Halt, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.CoreCount, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.InitCore, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.AbortCore, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.CoreStatus, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.Pop8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.Pop16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.Pop32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.Pop64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.PushI8, Arg < byte > ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.PushI16, Arg < ushort > ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.PushI32, Arg < uint > ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.PushI64, Arg < ulong > ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.PushSP, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.PushSF, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.MovSP, Arg < uint > ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.MovSFI8, Arg < uint > ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.MovSFI16, Arg < uint > ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.MovSFI32, Arg < uint > ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.MovSFI64, Arg < uint > ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.LoadI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.LoadI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.LoadI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.LoadN, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.LoadI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.StoreI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.StoreI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.StoreI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.StoreI64, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.StoreN, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.DupI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.DupI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.DupI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.DupI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.SwapI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.SwapI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.SwapI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.SwapI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.AddI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.AddI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.AddI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.AddI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.SubI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.SubI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.SubI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.SubI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.MulI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.MulI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.MulI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.MulI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.DivI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.DivI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.DivI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.DivI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ModI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ModI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ModI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ModI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.AndI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.AndI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.AndI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.AndI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.OrI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.OrI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.OrI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.OrI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.XorI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.XorI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.XorI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.XorI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.NotI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.NotI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.NotI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.NotI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ShlI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ShlI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ShlI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ShlI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ShrI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ShrI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ShrI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.ShrI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.Call, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.Jump, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.Return, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchZeroI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchZeroI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchZeroI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchZeroI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchNotZeroI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchNotZeroI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchNotZeroI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchNotZeroI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchLessI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchLessI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchLessI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchLessI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchGreaterI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchGreaterI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchGreaterI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchGreaterI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchLessOrEqualI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchLessOrEqualI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchLessOrEqualI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchLessOrEqualI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchGreaterOrEqualI8, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchGreaterOrEqualI16, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchGreaterOrEqualI32, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.BranchGreaterOrEqualI64, ZeroArgs ) );

            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.InteropCall, ZeroArgs ) );
            s_InstructionParsers.Add( new BadAssemblyInstructionParser( OpCode.InteropResolve, ZeroArgs ) );
        }

        private BadAssemblyInstructionParser( OpCode opCode, Action < OpCode, SourceReader, CodeSectionWriter > parser )
        {
            OpCode = opCode;
            m_Parser = parser;
        }

        private static void Arg < T >( OpCode op, SourceReader reader, CodeSectionWriter writer )
        {
            List < byte > bytes = new List < byte >();
            int off = sizeof( ushort ) + writer.CurrentSize;
            bytes.AddRange( ParseArg < T >( reader, writer, off ) );
            off += bytes.Count;
            writer.Emit( op, bytes.ToArray() );
        }

        private static void Arg < T0, T1 >( OpCode op, SourceReader reader, CodeSectionWriter writer )
        {
            List < byte > bytes = new List < byte >();
            int off = sizeof( ushort ) + writer.CurrentSize;
            bytes.AddRange( ParseArg < T0 >( reader, writer, off ) );
            off += bytes.Count;
            bytes.AddRange( ParseArg < T1 >( reader, writer, off ) );
            off += bytes.Count;
            writer.Emit( op, bytes.ToArray() );
        }

        private static byte[] ParseArg < T >( SourceReader reader, CodeSectionWriter writer, int argAddr )
        {
            if ( reader.IsHexNumberStart() )
            {
                SourceReaderToken token = reader.ParseHexNumber();
                decimal d = ( decimal )token.ParsedValue;
                T num = ConvertNumber < T >( d );

                if ( num == null )
                {
                    throw new ParseException( $"Invalid number: {d} for type {typeof( T ).Name}", token.SourceToken );
                }

                return GetBytes( num );
            }
            else if ( reader.IsNumberStart() )
            {
                SourceReaderToken token = reader.ParseNumber();
                decimal d = ( decimal )token.ParsedValue;

                T num = ConvertNumber < T >( d );

                if ( num == null )
                {
                    throw new ParseException( $"Invalid number: {d} for type {typeof( T ).Name}", token.SourceToken );
                }

                return GetBytes( num );
            }
            else
            {
                //Parse Assembly Symbol

                AssemblySymbol sym = reader.ParseSymbol(
                                                        writer.AssemblyName,
                                                        writer.SectionName,
                                                        out SourceToken token
                                                       );

                int sz = Marshal.SizeOf < T >();

                if ( sz != 8 )
                {
                    throw new ParseException( "Invalid size for symbol", token );
                }

                writer.AddPatchSite( sym, argAddr, sz );

                return new byte[sz];
            }
        }

        private static void ZeroArgs( OpCode op, SourceReader reader, CodeSectionWriter writer )
        {
            writer.Emit( op, Array.Empty < byte >() );
        }

        #endregion

    }

}
