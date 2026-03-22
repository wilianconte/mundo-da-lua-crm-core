using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record StudentGuardianDto(
    Guid Id,
    Guid TenantId,
    Guid StudentId,
    Guid GuardianPersonId,
    GuardianRelationshipType RelationshipType,
    bool IsPrimaryGuardian,
    bool IsFinancialResponsible,
    bool ReceivesNotifications,
    bool CanPickupChild,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
