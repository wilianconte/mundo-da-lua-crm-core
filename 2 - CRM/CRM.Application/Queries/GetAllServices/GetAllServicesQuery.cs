using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllServices;

public record GetAllServicesQuery : IRequest<Result<IReadOnlyList<ServiceDto>>>;
