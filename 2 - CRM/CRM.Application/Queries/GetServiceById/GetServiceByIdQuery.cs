using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetServiceById;

public record GetServiceByIdQuery(Guid Id) : IRequest<Result<ServiceDto>>;
