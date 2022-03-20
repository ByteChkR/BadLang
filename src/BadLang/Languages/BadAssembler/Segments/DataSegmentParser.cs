using BadVM.Shared.AssemblyFormat.Serialization;

namespace BadAssembler.Segments
{

    public class DataSegmentParser : SegmentParser
    {

        private readonly List < DataSyntaxParser > m_SyntaxParsers;

        public override IEnumerable < ISyntaxParser > SyntaxParsers => m_SyntaxParsers;

        #region Public

        public DataSegmentParser( List < DataSyntaxParser > syntaxParsers ) : base( "data" )
        {
            m_SyntaxParsers = syntaxParsers;
        }

        public override void Parse( SourceReader reader, AssemblyWriter writer, PostSegmentParseTasks taskList )
        {
            reader.SkipWhitespaceAndNewlineAndComments();
            SourceReaderToken name = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments();
            SourceReaderToken format = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments();
            SourceReaderToken language = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments();

            bool isReadOnly = false;

            if ( reader.Is( "readonly" ) )
            {
                reader.Eat( "readonly" );
                reader.SkipWhitespaceAndNewlineAndComments();
                isReadOnly = true;
            }

            reader.Eat( '{' );

            DataSyntaxParser syntaxParser = m_SyntaxParsers.Find( p => p.Name == language.StringValue );

            if ( syntaxParser == null )
            {
                throw new Exception( $"Unknown language '{language.StringValue}'" );
            }

            syntaxParser.Parse(
                               reader,
                               writer,
                               writer.GetDataWriter( name.StringValue, format.StringValue, isReadOnly ),
                               taskList
                              );

            reader.SkipWhitespaceAndNewlineAndComments();
            reader.Eat( '}' );
        }

        #endregion

    }

}
