namespace BadVM.Shared.Exceptions;

public abstract class RuntimeException : Exception
{

    #region Protected

    protected RuntimeException( string message ) : base( message )
    {
    }

    protected RuntimeException( string message, Exception innerException ) : base( message, innerException )
    {
    }

    #endregion

}
