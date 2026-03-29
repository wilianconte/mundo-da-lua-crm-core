using MediatR;
using Microsoft.Extensions.Logging;

namespace MyCRM.Shared.Kernel.Behaviors;

public sealed class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuditLoggingBehavior<TRequest, TResponse>> _logger;

    public AuditLoggingBehavior(ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var commandName = typeof(TRequest).Name;

        _logger.LogInformation("Audit: executing {CommandName}", commandName);

        var response = await next();

        _logger.LogInformation("Audit: {CommandName} completed", commandName);

        return response;
    }
}
