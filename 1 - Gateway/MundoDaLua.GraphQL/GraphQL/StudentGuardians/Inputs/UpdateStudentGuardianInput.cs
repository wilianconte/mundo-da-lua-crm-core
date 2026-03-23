using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.StudentGuardians.Inputs;

public record UpdateStudentGuardianInput(
    GuardianRelationshipType RelationshipType,
    bool IsPrimaryGuardian,
    bool IsFinancialResponsible,
    bool ReceivesNotifications,
    bool CanPickupChild,
    string? Notes
);
