using BadVM.Shared.AssemblyFormat.Serialization;

namespace BadAssembler.Segments
{

    public class CodeSegmentParser : SegmentParser
    {

        private readonly List < CodeSyntaxParser > m_SyntaxParsers;

        public override IEnumerable < ISyntaxParser > SyntaxParsers => m_SyntaxParsers;

        #region Public

        public CodeSegmentParser( List < CodeSyntaxParser > syntaxParsers ) : base( "code" )
        {
            m_SyntaxParsers = syntaxParsers;
        }

        public override void Parse( SourceReader reader, AssemblyWriter writer, PostSegmentParseTasks taskList )
        {
            reader.SkipWhitespaceAndNewlineAndComments();
            string sectionName = reader.ParseWord().StringValue;

            if ( reader.Is( ':' ) )
            {
                reader.Eat( ':' );
                SourceReaderToken subName = reader.ParseWord();
                sectionName += $":{subName.StringValue}";
                reader.SkipWhitespaceAndNewlineAndComments();
            }

            reader.SkipWhitespaceAndNewlineAndComments();
            SourceReaderToken format = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments();
            SourceReaderToken language = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments();

            reader.Eat( '{' );

            CodeSyntaxParser syntaxParser = m_SyntaxParsers.Find( p => p.Name == language.StringValue );

            if ( syntaxParser == null )
            {
                throw new Exception( $"Unknown language '{language.StringValue}'" );
            }

            syntaxParser.Parse( reader, writer.GetCodeWriter( sectionName, format.StringValue ), taskList );

            reader.SkipWhitespaceAndNewlineAndComments();
            reader.Eat( '}' );
        }

        #endregion

    }

}
