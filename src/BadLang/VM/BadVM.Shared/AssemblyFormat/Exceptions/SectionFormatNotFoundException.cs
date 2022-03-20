namespace BadVM.Shared.AssemblyFormat.Exceptions;

public class SectionFormatNotFoundException : Exception
{

    #region Public

    public SectionFormatNotFoundException( string message ) : base( message )
    {
    }

    #endregion

}
