namespace MyCRM.Shared.Kernel.Results;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Errors = [];
    }

    private Result(string errorCode, IReadOnlyList<string> errors)
    {
        IsSuccess = false;
        ErrorCode = errorCode;
        Errors = errors;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string errorCode, params string[] errors) =>
        new(errorCode, errors);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(bool success, string? errorCode = null, IReadOnlyList<string>? errors = null)
    {
        IsSuccess = success;
        ErrorCode = errorCode;
        Errors = errors ?? [];
    }

    public static Result Success() => new(true);

    public static Result Failure(string errorCode, params string[] errors) =>
        new(false, errorCode, errors);
}
