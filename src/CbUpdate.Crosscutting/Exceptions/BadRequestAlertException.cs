using CbUpdate.Crosscutting.Constants;

namespace CbUpdate.Crosscutting.Exceptions;

public class BadRequestAlertException : BaseException
{
    public BadRequestAlertException(string detail, string entityName, string errorKey) : this(
        ErrorConstants.DefaultType, detail, entityName, errorKey)
    {
    }

    public BadRequestAlertException(string type, string detail, string entityName, string errorKey) : base(type, detail, entityName, errorKey)
    {
    }
}
