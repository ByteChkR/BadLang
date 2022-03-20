using BadAssembler.Segments;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadAssembler.AssemblerSyntax
{

    public class BadAssemblyCodeParser : CodeSyntaxParser
    {

        #region Public

        public BadAssemblyCodeParser() : base( "BASM" )
        {
        }

        public override void Parse(
            SourceReader reader,
            CodeSectionWriter sectionWriter,
            PostSegmentParseTasks taskList )
        {
            reader.SkipWhitespaceAndNewlineAndComments();

            while ( !reader.Is( '}' ) )
            {
                ParseElement( reader, sectionWriter );
                reader.SkipWhitespaceAndNewlineAndComments();
            }
        }

        #endregion

        #region Private

        //Parse Element
        //  Parse Word
        //  if next is :
        //    return Parse Label
        //  return Parse Instruction

        //Parse Instruction Argument
        //  Number => Automatically Convert into right size
        //  Assembly Symbol => Always 64 bits, add as patch site

        private void ParseElement( SourceReader reader, CodeSectionWriter writer )
        {
            SourceReaderToken token = reader.ParseWord();

            reader.SkipWhitespace();

            if ( reader.Is( ':' ) )
            {
                reader.Eat( ':' );
                reader.SkipWhitespace();

                SourceReaderToken visibility = reader.ParseWord();

                writer.Label(
                             token.StringValue,
                             ( AssemblyElementVisibility )Enum.Parse(
                                                                     typeof( AssemblyElementVisibility ),
                                                                     visibility.StringValue,
                                                                     true
                                                                    )
                            );

                return;
            }

            OpCode opCode = ( OpCode )Enum.Parse( typeof( OpCode ), token.StringValue, true );
            BadAssemblyInstructionParser.ParseInstruction( opCode, token.SourceToken, reader, writer );
        }

        #endregion

    }

}
