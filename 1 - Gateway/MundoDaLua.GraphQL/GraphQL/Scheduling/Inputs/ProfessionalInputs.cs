using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Inputs;

public record CreateProfessionalInput(
    Guid PersonId,
    List<Guid> SpecialtyIds,
    string? Bio,
    string? LicenseNumber,
    decimal? CommissionPercentage);

public record UpdateProfessionalStatusInput(
    Guid Id,
    ProfessionalStatus TargetStatus);
