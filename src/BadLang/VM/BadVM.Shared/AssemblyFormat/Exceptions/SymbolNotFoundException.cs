namespace BadVM.Shared.AssemblyFormat.Exceptions;

public class SymbolNotFoundException : Exception
{

    #region Public

    public SymbolNotFoundException( string message ) : base( message )
    {
    }

    #endregion

}
