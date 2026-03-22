using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Students.UpdateStudent;

public sealed class UpdateStudentHandler : IRequestHandler<UpdateStudentCommand, Result<StudentDto>>
{
    private readonly IStudentRepository _repository;
    private readonly ITenantService     _tenant;

    public UpdateStudentHandler(IStudentRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant     = tenant;
    }

    public async Task<Result<StudentDto>> Handle(UpdateStudentCommand request, CancellationToken ct)
    {
        var student = await _repository.GetByIdAsync(request.Id, ct);
        if (student is null)
            return Result<StudentDto>.Failure("STUDENT_NOT_FOUND", "Student not found.");

        // Prevent duplicate registration numbers on update
        if (request.RegistrationNumber is not null)
        {
            var regExists = await _repository.RegistrationNumberExistsAsync(
                _tenant.TenantId, request.RegistrationNumber, excludeId: request.Id, ct: ct);
            if (regExists)
                return Result<StudentDto>.Failure(
                    "STUDENT_REGISTRATION_NUMBER_DUPLICATE",
                    "A student with this registration number already exists.");
        }

        student.UpdateInfo(
            registrationNumber:  request.RegistrationNumber,
            schoolName:          request.SchoolName,
            gradeOrClass:        request.GradeOrClass,
            enrollmentType:      request.EnrollmentType,
            unitId:              request.UnitId,
            classGroup:          request.ClassGroup,
            startDate:           request.StartDate,
            notes:               request.Notes,
            academicObservation: request.AcademicObservation);

        _repository.Update(student);
        await _repository.SaveChangesAsync(ct);

        return Result<StudentDto>.Success(student.Adapt<StudentDto>());
    }
}
