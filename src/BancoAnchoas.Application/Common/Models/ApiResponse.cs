namespace BancoAnchoas.Application.Common.Models;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string? Message { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null)
        => new() { Data = data, Message = message };
}
