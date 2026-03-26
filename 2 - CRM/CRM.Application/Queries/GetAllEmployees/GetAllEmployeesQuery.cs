using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllEmployees;

public record GetAllEmployeesQuery : IRequest<Result<IReadOnlyList<EmployeeDto>>>;
