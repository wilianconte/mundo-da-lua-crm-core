using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentGuardians.CreateStudentGuardian;

public sealed class CreateStudentGuardianHandler : IRequestHandler<CreateStudentGuardianCommand, Result<StudentGuardianDto>>
{
    private readonly IStudentGuardianRepository _repository;
    private readonly IStudentRepository         _studentRepository;
    private readonly IPersonRepository          _personRepository;
    private readonly ITenantService             _tenant;

    public CreateStudentGuardianHandler(
        IStudentGuardianRepository repository,
        IStudentRepository studentRepository,
        IPersonRepository personRepository,
        ITenantService tenant)
    {
        _repository        = repository;
        _studentRepository = studentRepository;
        _personRepository  = personRepository;
        _tenant            = tenant;
    }

    public async Task<Result<StudentGuardianDto>> Handle(CreateStudentGuardianCommand request, CancellationToken ct)
    {
        // Ensure student exists
        var student = await _studentRepository.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return Result<StudentGuardianDto>.Failure("STUDENT_NOT_FOUND", "Student not found.");

        // Ensure guardian person exists
        var guardianPerson = await _personRepository.GetByIdAsync(request.GuardianPersonId, ct);
        if (guardianPerson is null)
            return Result<StudentGuardianDto>.Failure("PERSON_NOT_FOUND", "Guardian person not found.");

        // Prevent duplicate guardian links for the same student
        var alreadyLinked = await _repository.GuardianAlreadyLinkedAsync(
            _tenant.TenantId, request.StudentId, request.GuardianPersonId, ct: ct);
        if (alreadyLinked)
            return Result<StudentGuardianDto>.Failure(
                "STUDENT_GUARDIAN_DUPLICATE",
                "This guardian is already linked to this student.");

        var studentGuardian = StudentGuardian.Create(
            tenantId:              _tenant.TenantId,
            studentId:             request.StudentId,
            guardianPersonId:      request.GuardianPersonId,
            relationshipType:      request.RelationshipType,
            isPrimaryGuardian:     request.IsPrimaryGuardian,
            isFinancialResponsible: request.IsFinancialResponsible,
            receivesNotifications: request.ReceivesNotifications,
            canPickupChild:        request.CanPickupChild,
            notes:                 request.Notes);

        await _repository.AddAsync(studentGuardian, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<StudentGuardianDto>.Success(studentGuardian.Adapt<StudentGuardianDto>());
    }
}
