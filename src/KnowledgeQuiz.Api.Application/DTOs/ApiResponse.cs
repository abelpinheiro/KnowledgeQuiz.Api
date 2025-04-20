namespace KnowledgeQuiz.Api.Application.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public ApiResponse(bool success, T? data = default, string? message = null)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null) => new(true, data, message);
    public static ApiResponse<T> Fail(string? message = null) => new(false, default, message);
}