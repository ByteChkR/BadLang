namespace BadVM.Shared.AssemblyFormat.Exceptions;

public class SectionNotFoundException : Exception
{

    #region Public

    public SectionNotFoundException( string message ) : base( message )
    {
    }

    #endregion

}
