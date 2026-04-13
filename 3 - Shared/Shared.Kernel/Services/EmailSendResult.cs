namespace MyCRM.Shared.Kernel.Services;

public sealed record EmailSendResult(bool IsSuccess, string? ErrorCode, string? ErrorMessage)
{
    public static EmailSendResult Success() => new(true, null, null);
    public static EmailSendResult Failure(string errorCode, string errorMessage) => new(false, errorCode, errorMessage);
}
