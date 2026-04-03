using System.Text.Json.Serialization;

namespace RevPay.API.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message };
    }

    public static ApiResponse<T> FailureResponse(string message)
    {
        return new ApiResponse<T> { Success = false, Message = message };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? message = null)
    {
        var response = new ApiResponse();
        ((ApiResponse<object>)response).Success = true;
        response.Message = message;
        return response;
    }

    public static ApiResponse Error(string message)
    {
        var response = new ApiResponse();
        ((ApiResponse<object>)response).Success = false;
        response.Message = message;
        return response;
    }
}
