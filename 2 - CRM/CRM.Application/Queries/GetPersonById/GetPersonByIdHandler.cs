using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetPersonById;

public sealed class GetPersonByIdHandler : IRequestHandler<GetPersonByIdQuery, Result<PersonDto>>
{
    private readonly IPersonRepository _repository;

    public GetPersonByIdHandler(IPersonRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PersonDto>> Handle(GetPersonByIdQuery request, CancellationToken ct)
    {
        var person = await _repository.GetByIdAsync(request.Id, ct);

        if (person is null)
            return Result<PersonDto>.Failure("PERSON_NOT_FOUND", "Person not found.");

        return Result<PersonDto>.Success(person.Adapt<PersonDto>());
    }
}
