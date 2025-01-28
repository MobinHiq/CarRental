namespace CarRental.Application.Responses;

public class BaseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> ValidationErrors { get; set; }

    public BaseResponse()
    {
        Success = true;
        ValidationErrors = new List<string>();
    }
}

public class BaseResponse<T> : BaseResponse
{
    public T Data { get; set; }
}