namespace MyCRM.Shared.Kernel.Services;

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? ToName = null,
    string? TextBody = null,
    string? From = null,
    string? FromName = null);
