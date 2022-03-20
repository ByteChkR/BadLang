using BadVM.Shared;

namespace BadAssembler.Exceptions;

public class ParseException : Exception
{

    public SourceToken Token { get; }

    #region Public

    public ParseException( string message, SourceToken token ) : base( message + " at " + token )
    {
        Token = token;
    }

    #endregion

}
