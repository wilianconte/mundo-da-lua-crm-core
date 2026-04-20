using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Inputs;

public record CreatePatientInput(
    Guid PersonId,
    string? Notes);

public record UpdatePatientStatusInput(
    Guid Id,
    PatientStatus TargetStatus);
