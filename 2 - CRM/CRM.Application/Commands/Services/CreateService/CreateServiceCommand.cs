using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Services.CreateService;

public sealed record CreateServiceCommand(
    string Name,
    decimal DefaultPrice,
    int DefaultDurationInMinutes,
    string? Description
) : IRequest<Result<ServiceDto>>;
