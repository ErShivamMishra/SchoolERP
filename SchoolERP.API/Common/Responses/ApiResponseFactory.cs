namespace SchoolERP.API.Common.Responses;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(T data, string message)
    {
        return ApiResponse<T>.Ok(data, message);
    }

    public static ApiResponse<object> Error(string message, params string[] errors)
    {
        return ApiResponse<object>.Fail(message, errors);
    }
}
