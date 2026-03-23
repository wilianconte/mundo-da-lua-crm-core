using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentGuardians.UpdateStudentGuardian;

public sealed class UpdateStudentGuardianHandler : IRequestHandler<UpdateStudentGuardianCommand, Result<StudentGuardianDto>>
{
    private readonly IStudentGuardianRepository _repository;

    public UpdateStudentGuardianHandler(IStudentGuardianRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<StudentGuardianDto>> Handle(UpdateStudentGuardianCommand request, CancellationToken ct)
    {
        var guardian = await _repository.GetByIdAsync(request.Id, ct);
        if (guardian is null)
            return Result<StudentGuardianDto>.Failure("STUDENT_GUARDIAN_NOT_FOUND", "Student guardian not found.");

        guardian.UpdateRelationship(
            relationshipType:      request.RelationshipType,
            isPrimaryGuardian:     request.IsPrimaryGuardian,
            isFinancialResponsible: request.IsFinancialResponsible,
            receivesNotifications: request.ReceivesNotifications,
            canPickupChild:        request.CanPickupChild,
            notes:                 request.Notes);

        _repository.Update(guardian);
        await _repository.SaveChangesAsync(ct);

        return Result<StudentGuardianDto>.Success(guardian.Adapt<StudentGuardianDto>());
    }
}
