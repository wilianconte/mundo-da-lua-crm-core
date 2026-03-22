using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentGuardians.UpdateStudentGuardian;

public record UpdateStudentGuardianCommand(
    Guid Id,
    GuardianRelationshipType RelationshipType,
    bool IsPrimaryGuardian,
    bool IsFinancialResponsible,
    bool ReceivesNotifications,
    bool CanPickupChild,
    string? Notes
) : IRequest<Result<StudentGuardianDto>>;
