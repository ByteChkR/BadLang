namespace BadVM.Interop;

public class InteropFunctionNotFoundException : Exception
{

    #region Public

    public InteropFunctionNotFoundException( string message ) : base( message )
    {
    }

    #endregion

}
