using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.People.DeletePerson;

public record DeletePersonCommand(Guid Id) : IRequest<Result>;
