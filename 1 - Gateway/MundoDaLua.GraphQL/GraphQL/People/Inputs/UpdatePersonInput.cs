using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.People.Inputs;

public record UpdatePersonInput(
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
);
