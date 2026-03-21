using MyCRM.CRM.Domain.Repositories;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.People.DeletePerson;

public sealed class DeletePersonHandler : IRequestHandler<DeletePersonCommand, Result>
{
    private readonly IPersonRepository _repository;

    public DeletePersonHandler(IPersonRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeletePersonCommand request, CancellationToken ct)
    {
        var person = await _repository.GetByIdAsync(request.Id, ct);

        if (person is null)
            return Result.Failure("PERSON_NOT_FOUND", "Person not found.");

        _repository.Delete(person);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
