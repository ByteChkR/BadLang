using BadAssembler.Segments;

using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadAssembler.AssemblerSyntax
{

    public class BadAssemblyDataParser : DataSyntaxParser
    {

        #region Public

        public BadAssemblyDataParser() : base( "BASM_DATA" )
        {
        }

        public override void Parse(
            SourceReader reader,
            AssemblyWriter writer,
            DataSectionWriter sectionWriter,
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

        private static void ParseElement( SourceReader reader, DataSectionWriter writer )
        {
            reader.Eat( '.' );
            SourceReaderToken elementType = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments();
            SourceReaderToken name = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments();
            SourceReaderToken vis = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments();

            AssemblyElementVisibility visibility =
                ( AssemblyElementVisibility )Enum.Parse( typeof( AssemblyElementVisibility ), vis.StringValue, true );

            BadAssemblyDataElementParser.ParseElement(
                                                      elementType.SourceToken,
                                                      elementType.StringValue,
                                                      name.StringValue,
                                                      visibility,
                                                      reader,
                                                      writer
                                                     );
        }

        #endregion

    }

}
