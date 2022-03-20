using BadAssembler.Segments;

using BadVM.Shared.AssemblyFormat.Serialization;

namespace BadAssembler
{

    public abstract class SegmentParser
    {

        public string Name { get; }

        public abstract IEnumerable < ISyntaxParser > SyntaxParsers { get; }

        #region Public

        public abstract void Parse( SourceReader reader, AssemblyWriter writer, PostSegmentParseTasks taskList );

        #endregion

        #region Protected

        protected SegmentParser( string name )
        {
            Name = name;
        }

        #endregion

    }

}
