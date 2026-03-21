using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.People.UpdatePerson;

public record UpdatePersonCommand(
    Guid Id,
    string FullName,
    string? PreferredName,
    string? DocumentNumber,
    DateOnly? BirthDate,
    Gender? Gender,
    MaritalStatus? MaritalStatus,
    string? Nationality,
    string? Occupation,
    string? Email,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WhatsAppNumber,
    string? ProfileImageUrl,
    string? Notes
) : IRequest<Result<PersonDto>>;
