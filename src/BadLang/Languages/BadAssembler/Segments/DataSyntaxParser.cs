using BadVM.Shared.AssemblyFormat.Serialization;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadAssembler.Segments
{

    public abstract class DataSyntaxParser : ISyntaxParser
    {

        public string Name { get; }

        #region Public

        public abstract void Parse(
            SourceReader reader,
            AssemblyWriter writer,
            DataSectionWriter sectionWriter,
            PostSegmentParseTasks taskList );

        #endregion

        #region Protected

        protected DataSyntaxParser( string name )
        {
            Name = name;
        }

        #endregion

    }

}
