namespace AfriPay.APP.Common.Models
{
    /// <summary>
    /// Common response pattern for application layer operations
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure => !IsSuccess;
        public T? Data { get; private set; }
        public T? Value => Data;
        public string? ErrorMessage { get; private set; }
        public string? Error => ErrorMessage;
        public List<string> Errors { get; private set; } = new();

        public static Result<T> Success(T data) => new()
        {
            IsSuccess = true,
            Data = data
        };

        public static Result<T> Failure(string errorMessage) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Errors = new List<string> { errorMessage }
        };

        public static Result<T> Failure(List<string> errors) => new()
        {
            IsSuccess = false,
            ErrorMessage = string.Join("; ", errors),
            Errors = errors
        };
    }
}
