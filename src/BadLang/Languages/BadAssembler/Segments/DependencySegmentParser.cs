using BadVM.Shared.AssemblyFormat.Serialization;

namespace BadAssembler.Segments
{

    public class DependencySegmentParser : SegmentParser
    {

        public override IEnumerable < ISyntaxParser > SyntaxParsers => Enumerable.Empty < ISyntaxParser >();

        #region Public

        public DependencySegmentParser() : base( "dependency" )
        {
        }

        public override void Parse( SourceReader reader, AssemblyWriter writer, PostSegmentParseTasks taskList )
        {
            reader.SkipWhitespaceAndNewlineAndComments();
            SourceReaderToken name = reader.ParseWord();
            writer.AddDependency( name.StringValue );
        }

        #endregion

    }

}
