using BadAssembler.Exceptions;

using BadC.Types;

using BadVM.Shared;

namespace BadC;

public class TypeMismatchException : ParseException
{

    #region Public

    public TypeMismatchException( BadCType actual, BadCType type, SourceToken token ) : base(
         $"Can not convert {actual} to {type}",
         token
        )
    {
    }

    #endregion

}
