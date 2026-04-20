using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Services.UpdateService;

public sealed record UpdateServiceCommand(
    Guid Id,
    string Name,
    decimal DefaultPrice,
    int DefaultDurationInMinutes,
    string? Description
) : IRequest<Result<ServiceDto>>;
