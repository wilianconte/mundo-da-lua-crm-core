using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.People.UpdatePerson;

public sealed class UpdatePersonHandler : IRequestHandler<UpdatePersonCommand, Result<PersonDto>>
{
    private readonly IPersonRepository _repository;
    private readonly ITenantService _tenant;

    public UpdatePersonHandler(IPersonRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result<PersonDto>> Handle(UpdatePersonCommand request, CancellationToken ct)
    {
        var person = await _repository.GetByIdAsync(request.Id, ct);

        if (person is null)
            return Result<PersonDto>.Failure("PERSON_NOT_FOUND", "Person not found.");

        if (request.Email is not null)
        {
            var emailExists = await _repository.EmailExistsAsync(_tenant.TenantId, request.Email, request.Id, ct);
            if (emailExists)
                return Result<PersonDto>.Failure("PERSON_EMAIL_DUPLICATE", "A person with this email already exists.");
        }

        if (request.DocumentNumber is not null)
        {
            var docExists = await _repository.DocumentNumberExistsAsync(_tenant.TenantId, request.DocumentNumber, request.Id, ct);
            if (docExists)
                return Result<PersonDto>.Failure("PERSON_DOCUMENT_DUPLICATE", "A person with this document number already exists.");
        }

        person.UpdateProfile(
            request.FullName,
            request.PreferredName,
            request.BirthDate,
            request.Gender,
            request.MaritalStatus,
            request.Nationality,
            request.Occupation,
            request.ProfileImageUrl,
            request.Notes);

        person.UpdateContact(
            request.Email,
            request.PrimaryPhone,
            request.SecondaryPhone,
            request.WhatsAppNumber);

        person.UpdateDocument(request.DocumentNumber);

        _repository.Update(person);
        await _repository.SaveChangesAsync(ct);

        return Result<PersonDto>.Success(person.Adapt<PersonDto>());
    }
}
