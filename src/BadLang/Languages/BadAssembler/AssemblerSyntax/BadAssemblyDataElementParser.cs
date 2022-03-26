using BadAssembler.Exceptions;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadAssembler.AssemblerSyntax
{

    public class BadAssemblyDataElementParser
    {

        private static readonly List < BadAssemblyDataElementParser > s_DataParsers =
            new List < BadAssemblyDataElementParser >
            {
                new BadAssemblyDataElementParser( ParseNumber < sbyte >, "i8" ),
                new BadAssemblyDataElementParser( ParseNumber < short >, "i16" ),
                new BadAssemblyDataElementParser( ParseNumber < int >, "i32" ),
                new BadAssemblyDataElementParser( ParseNumber < long >, "i64" ),
                new BadAssemblyDataElementParser( ParseNumber < byte >, "u8" ),
                new BadAssemblyDataElementParser( ParseNumber < ushort >, "u16" ),
                new BadAssemblyDataElementParser( ParseNumber < uint >, "u32" ),
                new BadAssemblyDataElementParser( ParseNumber < ulong >, "u64" ),
                new BadAssemblyDataElementParser( ParseNumber < float >, "f32" ),
                new BadAssemblyDataElementParser( ParseNumber < double >, "f64" ),
                new BadAssemblyDataElementParser( ParseString, "cstr" ),
                new BadAssemblyDataElementParser( ParseFile, "file" ),
                new BadAssemblyDataElementParser( ParseArray, "array" )
            };

        private readonly Action < string, AssemblyElementVisibility, SourceReader, DataSectionWriter > m_Parser;

        public string Name { get; }

        #region Public

        public BadAssemblyDataElementParser(
            Action < string, AssemblyElementVisibility, SourceReader, DataSectionWriter > parser,
            string name )
        {
            m_Parser = parser;
            Name = name;
        }

        public static void ParseElement(
            SourceToken typeToken,
            string type,
            string name,
            AssemblyElementVisibility visibility,
            SourceReader reader,
            DataSectionWriter writer )
        {
            BadAssemblyDataElementParser parser = s_DataParsers.FirstOrDefault( p => p.Name == type );

            if ( parser == null )
            {
                throw new ParseException( $"Unknown data element type {type}", typeToken );
            }

            parser.Parse( reader, visibility, writer, name );
        }

        public void Parse(
            SourceReader reader,
            AssemblyElementVisibility visibility,
            DataSectionWriter writer,
            string elemName )
        {
            m_Parser( elemName, visibility, reader, writer );
        }

        #endregion

        #region Private

        private static void ParseArray(
            string name,
            AssemblyElementVisibility visibility,
            SourceReader reader,
            DataSectionWriter writer )
        {
            SourceReaderToken typeToken = reader.ParseWord();

            reader.SkipWhitespaceAndNewlineAndComments();
            int elementSize;

            switch ( typeToken.StringValue )
            {
                case "i8":
                    elementSize = 1;

                    break;

                case "i16":
                    elementSize = 2;

                    break;

                case "i32":
                    elementSize = 4;

                    break;

                case "i64":
                    elementSize = 8;

                    break;

                default:
                    throw new ParseException(
                                             $"Unknown array element type {typeToken.StringValue}",
                                             typeToken.SourceToken
                                            );
            }

            long num;

            if ( reader.IsHexNumberStart() )
            {
                SourceReaderToken token = reader.ParseHexNumber();
                decimal d = ( decimal )token.ParsedValue;

                if ( d < 0 )
                {
                    throw new ParseException( $"Invalid number {d} for array size", token.SourceToken );
                }

                num = BadAssemblyInstructionParser.
                    ConvertNumber < long >( d );
            }
            else if ( reader.IsNumberStart() )
            {
                SourceReaderToken token = reader.ParseNumber();
                decimal d = ( decimal )token.ParsedValue;

                if ( d < 0 )
                {
                    throw new ParseException( $"Invalid number {d} for array size", token.SourceToken );
                }

                num = BadAssemblyInstructionParser.
                    ConvertNumber < long >( d );
            }
            else
            {
                throw new ParseException( "Expected number", reader.SourceFile.GetToken( reader.CurrentIndex ) );
            }

            writer.AddData(
                           name,
                           visibility,
                           new byte[( int )num * elementSize]
                          );
        }

        private static void ParseFile(
            string name,
            AssemblyElementVisibility visibility,
            SourceReader reader,
            DataSectionWriter writer )
        {
            SourceReaderToken str = reader.ParseString();
            string s = str.ParsedValue.ToString();
            byte[] data = File.ReadAllBytes( s );
            writer.AddData( name, visibility, data );
            writer.AddData( name + "_size", visibility, BitConverter.GetBytes( data.Length ) );
        }

        private static void ParseNumber < T >(
            string name,
            AssemblyElementVisibility visibility,
            SourceReader reader,
            DataSectionWriter writer )
        {
            if ( reader.IsHexNumberStart() )
            {
                SourceReaderToken token = reader.ParseHexNumber();
                decimal d = ( decimal )token.ParsedValue;

                object? num = BadAssemblyInstructionParser.
                    ConvertNumber < T >( d );

                if ( num == null )
                {
                    throw new ParseException( $"Invalid number {d} for type {typeof( T ).Name}", token.SourceToken );
                }

                writer.AddData(
                               name,
                               visibility,
                               BadAssemblyInstructionParser.GetBytes(
                                                                     num
                                                                    )
                              );
            }
            else if ( reader.IsNumberStart() )
            {
                SourceReaderToken token = reader.ParseNumber();
                decimal d = ( decimal )token.ParsedValue;

                T num = BadAssemblyInstructionParser.
                    ConvertNumber < T >( d );

                if ( num == null )
                {
                    throw new ParseException( $"Invalid number {d} for type {typeof( T ).Name}", token.SourceToken );
                }

                writer.AddData(
                               name,
                               visibility,
                               BadAssemblyInstructionParser.GetBytes(
                                                                     num
                                                                    )
                              );
            }
            else
            {
                throw new ParseException( "Expected number", reader.SourceFile.GetToken( reader.CurrentIndex ) );
            }
        }

        private static void ParseString(
            string name,
            AssemblyElementVisibility visibility,
            SourceReader reader,
            DataSectionWriter writer )
        {
            SourceReaderToken str = reader.ParseString();
            string s = str.ParsedValue.ToString() + '\0';
            byte[] data = s.ToCharArray().SelectMany( BitConverter.GetBytes ).ToArray();
            writer.AddData( name, visibility, data );
        }

        #endregion

    }

}
