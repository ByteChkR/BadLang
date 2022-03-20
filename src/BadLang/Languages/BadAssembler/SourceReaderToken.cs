using BadVM.Shared;

namespace BadAssembler
{

    public class SourceReaderToken
    {

        public int End { get; }

        public SourceReader Reader { get; }

        public SourceToken SourceToken { get; }

        public string StringValue => Reader.Source.Substring( Start, End - Start );

        public object ParsedValue { get; private set; }

        #region Unity Event Functions

        public int Start { get; }

        #endregion

        #region Public

        public SourceReaderToken( int start, int end, SourceReader reader, object parsedValue, SourceToken token )
        {
            Start = start;
            End = end;
            Reader = reader;
            ParsedValue = parsedValue;
            SourceToken = token;
        }

        public SourceReaderToken( int start, int end, SourceReader reader, SourceToken token )
        {
            Start = start;
            End = end;
            Reader = reader;
            SourceToken = token;
            ParsedValue = StringValue;
        }

        public void SetParsedValue( object value )
        {
            ParsedValue = value;
        }

        #endregion

    }

}
