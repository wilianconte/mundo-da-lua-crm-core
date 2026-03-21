using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.People.CreatePerson;

public sealed class CreatePersonHandler : IRequestHandler<CreatePersonCommand, Result<PersonDto>>
{
    private readonly IPersonRepository _repository;
    private readonly ITenantService _tenant;

    public CreatePersonHandler(IPersonRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result<PersonDto>> Handle(CreatePersonCommand request, CancellationToken ct)
    {
        if (request.Email is not null)
        {
            var emailExists = await _repository.EmailExistsAsync(_tenant.TenantId, request.Email, ct: ct);
            if (emailExists)
                return Result<PersonDto>.Failure("PERSON_EMAIL_DUPLICATE", "A person with this email already exists.");
        }

        if (request.DocumentNumber is not null)
        {
            var docExists = await _repository.DocumentNumberExistsAsync(_tenant.TenantId, request.DocumentNumber, ct: ct);
            if (docExists)
                return Result<PersonDto>.Failure("PERSON_DOCUMENT_DUPLICATE", "A person with this document number already exists.");
        }

        var person = Person.Create(
            tenantId: _tenant.TenantId,
            fullName: request.FullName,
            email: request.Email,
            documentNumber: request.DocumentNumber,
            birthDate: request.BirthDate,
            preferredName: request.PreferredName,
            primaryPhone: request.PrimaryPhone,
            secondaryPhone: request.SecondaryPhone,
            whatsAppNumber: request.WhatsAppNumber,
            gender: request.Gender,
            maritalStatus: request.MaritalStatus,
            nationality: request.Nationality,
            occupation: request.Occupation,
            profileImageUrl: request.ProfileImageUrl,
            notes: request.Notes);

        await _repository.AddAsync(person, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<PersonDto>.Success(person.Adapt<PersonDto>());
    }
}
