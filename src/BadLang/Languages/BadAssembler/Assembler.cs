using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Exceptions;
using BadVM.Shared.AssemblyFormat.Serialization;
using BadVM.Shared.Logging;

namespace BadAssembler
{

    public class Assembler
    {

        public static readonly LogMask LogMask = new LogMask( "BadAssembler" );

        private readonly List < SegmentParser > m_SegmentParsers = new List < SegmentParser >();

        #region Public

        public Assembler( List < SegmentParser > segmentParsers )
        {
            m_SegmentParsers = segmentParsers;
        }

        public (byte[], AssemblyCompilationDataContainer) Assemble(
            IEnumerable < SourceFile > sources,
            AssemblyWriter writer )
        {
            PostSegmentParseTasks taskList = new PostSegmentParseTasks();

            LogMask.LogMessage( "Processing Source Files..." );

            foreach ( SourceFile src in sources )
            {
                Parse( src, writer, taskList );
            }

            LogMask.LogMessage( "Processing Post-Parse Tasks..." );

            taskList.Run();

            LogMask.LogMessage( "Writing Assembly..." );

            return ( writer.GetAssembly(), writer.CompilationData );
        }

        #endregion

        #region Private

        private void Parse( SourceFile src, AssemblyWriter writer, PostSegmentParseTasks taskList )
        {
            SourceReader reader = new SourceReader( src );

            //Parse Segments
            reader.SkipWhitespaceAndNewlineAndComments();

            while ( !reader.IsEoF )
            {
                ParseSegment( reader, writer, taskList );
                reader.SkipWhitespaceAndNewlineAndComments();
            }
        }

        private void ParseSegment( SourceReader reader, AssemblyWriter writer, PostSegmentParseTasks taskList )
        {
            reader.Eat( '.' );

            SourceReaderToken token = reader.ParseWord();

            string segmentType = token.StringValue;

            SegmentParser segmentParser = m_SegmentParsers.FirstOrDefault( p => p.Name == segmentType );

            if ( segmentParser == null )
            {
                throw new SectionFormatNotFoundException( $"Unknown segment type '{segmentType}'" );
            }

            segmentParser.Parse( reader, writer, taskList );
        }

        #endregion

    }

}
