using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Services.DeactivateService;

public sealed record DeactivateServiceCommand(Guid Id) : IRequest<Result<ServiceDto>>;
