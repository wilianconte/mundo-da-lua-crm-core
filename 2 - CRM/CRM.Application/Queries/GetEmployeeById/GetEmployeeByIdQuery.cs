using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<Result<EmployeeDto>>;
