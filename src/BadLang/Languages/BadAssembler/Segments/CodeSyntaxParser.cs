using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadAssembler.Segments
{

    public abstract class CodeSyntaxParser : ISyntaxParser
    {

        public string Name { get; }

        #region Public

        public abstract void Parse(
            SourceReader reader,
            CodeSectionWriter sectionWriter,
            PostSegmentParseTasks taskList );

        #endregion

        #region Protected

        protected CodeSyntaxParser( string name )
        {
            Name = name;
        }

        #endregion

    }

}
