using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using BadAssembler.Exceptions;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadAssembler
{

    public class SourceReader
    {

        public SourceFile SourceFile { get; }

        public string Source => SourceFile.Source;

        public int CurrentIndex { get; set; }

        public bool IsEoF => CurrentIndex >= Source.Length;

        public char Current => Get( 0 );

        public bool IsSymbol => char.IsSymbol( Current ) || Current == '(' || Current == ')';

        #region Public

        public SourceReader( SourceFile source )
        {
            SourceFile = source;
        }

        public static bool IsWhiteSpace( char c )
        {
            return char.IsWhiteSpace( c ) || c == '\r';
        }

        public static bool IsWordStart( char c )
        {
            return char.IsLetter( c ) || c == '_';
        }

        public void Eat( string str )
        {
            foreach ( char c in str )
            {
                Eat( c );
            }
        }

        public void Eat( char c )
        {
            if ( !Is( c ) )
            {
                throw new ParseException(
                                         $"Expected {c} at index {CurrentIndex} but found {Current}",
                                         SourceFile.GetToken( CurrentIndex )
                                        );
            }

            CurrentIndex++;
        }

        public char Get( int off )
        {
            int i = CurrentIndex + off;

            if ( i >= Source.Length )
            {
                return '\0';
            }

            return Source[i];
        }

        public bool Is( char c )
        {
            return Current == c;
        }

        public bool Is( string c )
        {
            for ( int i = 0; i < c.Length; i++ )
            {
                if ( c[i] != Get( i ) )
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsDigit()
        {
            return char.IsDigit( Current );
        }

        public bool IsHexNumberStart()
        {
            return Is( "0x" );
        }

        public bool IsNumberStart()
        {
            return Is( '-' ) || char.IsDigit( Current );
        }

        public bool IsWhiteSpace()
        {
            return IsWhiteSpace( Current );
        }

        public bool IsWordMiddle()
        {
            return char.IsLetterOrDigit( Current ) || Current == '_';
        }

        public bool IsWordStart()
        {
            return IsWordStart( Current );
        }

        public SourceReaderToken ParseDigits()
        {
            int start = CurrentIndex;
            SourceToken token = SourceFile.GetToken( CurrentIndex );
            SkipWhile( c => c >= '0' && c <= '9' );

            return new SourceReaderToken( start, CurrentIndex, this, token );
        }

        public SourceReaderToken ParseHexDigits()
        {
            int start = CurrentIndex;
            SourceToken token = SourceFile.GetToken( CurrentIndex );
            SkipWhile( c => c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F' );

            return new SourceReaderToken( start, CurrentIndex, this, token );
        }

        public SourceReaderToken ParseHexNumber()
        {
            int start = CurrentIndex;

            SourceToken t = SourceFile.GetToken( CurrentIndex );

            Eat( '0' );
            Eat( 'x' );

            SourceReaderToken token = ParseHexDigits();

            return new SourceReaderToken(
                                         start,
                                         CurrentIndex,
                                         this,
                                         ( decimal )long.Parse( token.StringValue, NumberStyles.HexNumber ),
                                         t
                                        );
        }

        public SourceReaderToken ParseNumber()
        {
            int mul = 1;

            SourceToken token = SourceFile.GetToken( CurrentIndex );

            if ( Is( '-' ) )
            {
                mul = -1;
                Eat( '-' );
            }

            SourceReaderToken digits = ParseDigits();
            StringBuilder sb = new StringBuilder( digits.StringValue );
            SourceReaderToken? fraction = null;

            if ( Is( '.' ) )
            {
                Eat( '.' );
                sb.Append( '.' );
                fraction = ParseDigits();
                sb.Append( fraction.StringValue );
            }

            return new SourceReaderToken(
                                         digits.Start,
                                         CurrentIndex,
                                         this,
                                         ( decimal )double.Parse( sb.ToString() ) * mul,
                                         token
                                        );
        }

        public SourceReaderToken ParseSequence( char[] chars )
        {
            int start = CurrentIndex;

            SourceToken token = SourceFile.GetToken( CurrentIndex );
            SkipWhile( chars.Contains );

            return new SourceReaderToken( start, CurrentIndex, this, token );
        }

        public SourceReaderToken ParseString()
        {
            SourceToken token = SourceFile.GetToken( CurrentIndex );

            if ( !Is( '\"' ) )
            {
                throw new ParseException( "Expected '\"'", token );
            }

            int pos = CurrentIndex;
            Eat( '\"' );
            StringBuilder sb = new StringBuilder();
            bool isEscaped = false;

            while ( !Is( '\"' ) && !Is( '\n' ) )
            {
                if ( Current == '\\' )
                {
                    isEscaped = true;
                }
                else
                {
                    sb.Append( Current );
                }

                Eat( Current );

                if ( isEscaped )
                {
                    string s = Regex.Unescape( "\\" + Current );
                    sb.Append( s );

                    Eat( Current );
                    isEscaped = false;
                }
            }

            Eat( '\"' );
            string str = sb.ToString();

            return new SourceReaderToken( pos, CurrentIndex, this, str, token );
        }

        //Parse String
        //  Handle Escape

        public AssemblySymbol ParseSymbol( string asm, string section, out SourceToken token )
        {
            List < string > parts = new List < string >();

            token = SourceFile.GetToken( CurrentIndex );

            for ( int i = 0; i < 3; i++ )
            {
                parts.Add( ParseWord().StringValue );

                if ( !Is( "::" ) )
                {
                    if ( Is( ':' ) )
                    {
                        Eat( ':' );
                        parts[parts.Count - 1] = parts[parts.Count - 1] + ":" + ParseWord().StringValue;

                        if ( !Is( "::" ) )
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                Eat( ':' );
                Eat( ':' );
            }

            return AssemblySymbol.Parse( string.Join( "::", parts ), asm, section );
        }

        public SourceReaderToken ParseUntilWhiteSpace()
        {
            int start = CurrentIndex;
            SourceToken token = SourceFile.GetToken( CurrentIndex );
            SkipUntil( IsWhiteSpace );

            return new SourceReaderToken( start, CurrentIndex, this, token );
        }

        public SourceReaderToken ParseUntilWhiteSpaceOrWordOrDigitOrSeparator()
        {
            int start = CurrentIndex;
            SourceToken token = SourceFile.GetToken( CurrentIndex );
            SkipUntil( x => IsWhiteSpace( x ) || IsWordStart( x ) || IsDigit() || x == ';' || x == ',' || x == ')' );

            return new SourceReaderToken( start, CurrentIndex, this, token );
        }

        public SourceReaderToken ParseWord()
        {
            SourceToken token = SourceFile.GetToken( CurrentIndex );

            if ( !IsWordStart() )
            {
                throw new ParseException( "Expected word", token );
            }

            int start = CurrentIndex;

            StringBuilder sb = new StringBuilder();

            while ( IsWordMiddle() )
            {
                sb.Append( Current );
                Eat( Current );
            }

            return new SourceReaderToken( start, CurrentIndex, this, sb.ToString(), token );
        }

        public void ResetTo( int index )
        {
            CurrentIndex = index;
        }

        public void ResetTo( SourceReaderToken token )
        {
            ResetTo( token.Start );
        }

        public void Skip( int i )
        {
            for ( int j = 0; j < i; j++ )
            {
                Eat( Current );
            }
        }

        public void SkipUntil( Func < char, bool > predicate )
        {
            while ( !predicate( Current ) )
            {
                Eat( Current );
            }
        }

        public void SkipWhile( Func < char, bool > predicate )
        {
            while ( predicate( Current ) )
            {
                Eat( Current );
            }
        }

        public void SkipWhitespace()
        {
            SkipWhile( c => IsWhiteSpace( c ) );
        }

        public void SkipWhitespaceAndComments()
        {
            while ( true )
            {
                SkipWhitespace();

                if ( Is( ';' ) )
                {
                    SkipUntil( c => c == '\n' );
                }
                else
                {
                    break;
                }
            }
        }

        public void SkipWhitespaceAndNewline()
        {
            SkipWhile( c => IsWhiteSpace( c ) || c == '\n' );
        }

        public void SkipWhitespaceAndNewlineAndComments( string commentChar = ";" )
        {
            while ( true )
            {
                SkipWhitespaceAndNewline();

                if ( Is( commentChar ) )
                {
                    SkipUntil( c => c == '\n' );
                }
                else
                {
                    break;
                }
            }
        }

        #endregion

    }

}
