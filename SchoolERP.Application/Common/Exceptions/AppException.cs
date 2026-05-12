namespace SchoolERP.Application.Common.Exceptions;

public class AppException : Exception
{
    public AppException(string message, int statusCode, string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public int StatusCode { get; }
    public string ErrorCode { get; }
}
