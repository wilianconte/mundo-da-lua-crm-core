using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentCourses.UpdateStudentCourse;

public sealed class UpdateStudentCourseHandler : IRequestHandler<UpdateStudentCourseCommand, Result<StudentCourseDto>>
{
    private readonly IStudentCourseRepository _repository;

    public UpdateStudentCourseHandler(IStudentCourseRepository repository) => _repository = repository;

    public async Task<Result<StudentCourseDto>> Handle(UpdateStudentCourseCommand request, CancellationToken ct)
    {
        var enrollment = await _repository.GetByIdAsync(request.Id, ct);
        if (enrollment is null)
            return Result<StudentCourseDto>.Failure("STUDENT_COURSE_NOT_FOUND", "Enrollment not found.");

        enrollment.UpdateInfo(
            enrollmentDate:     request.EnrollmentDate,
            startDate:          request.StartDate,
            endDate:            request.EndDate,
            classGroup:         request.ClassGroup,
            shift:              request.Shift,
            scheduleDescription: request.ScheduleDescription,
            unitId:             request.UnitId,
            notes:              request.Notes);

        _repository.Update(enrollment);
        await _repository.SaveChangesAsync(ct);

        return Result<StudentCourseDto>.Success(enrollment.Adapt<StudentCourseDto>());
    }
}
