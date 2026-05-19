namespace DATN_70.Services;

public sealed class ServiceResult<T>
{
    private ServiceResult(bool success, T? data, string? errorMessage)
    {
        Success = success;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public T? Data { get; }

    public string? ErrorMessage { get; }

    public static ServiceResult<T> Ok(T data) => new(true, data, null);

    public static ServiceResult<T> Fail(string errorMessage) => new(false, default, errorMessage);
}
