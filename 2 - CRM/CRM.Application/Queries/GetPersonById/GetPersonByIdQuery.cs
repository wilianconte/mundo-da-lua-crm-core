using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetPersonById;

public record GetPersonByIdQuery(Guid Id) : IRequest<Result<PersonDto>>;
