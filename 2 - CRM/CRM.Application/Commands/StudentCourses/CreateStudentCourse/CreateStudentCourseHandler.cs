using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentCourses.CreateStudentCourse;

public sealed class CreateStudentCourseHandler : IRequestHandler<CreateStudentCourseCommand, Result<StudentCourseDto>>
{
    private readonly IStudentCourseRepository _repository;
    private readonly IStudentRepository       _studentRepository;
    private readonly ICourseRepository        _courseRepository;
    private readonly ITenantService           _tenant;

    public CreateStudentCourseHandler(
        IStudentCourseRepository repository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        ITenantService tenant)
    {
        _repository        = repository;
        _studentRepository = studentRepository;
        _courseRepository  = courseRepository;
        _tenant            = tenant;
    }

    public async Task<Result<StudentCourseDto>> Handle(CreateStudentCourseCommand request, CancellationToken ct)
    {
        // Ensure the Student exists within this tenant
        var student = await _studentRepository.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return Result<StudentCourseDto>.Failure("STUDENT_NOT_FOUND", "Student not found.");

        // Ensure the Course exists within this tenant
        var course = await _courseRepository.GetByIdAsync(request.CourseId, ct);
        if (course is null)
            return Result<StudentCourseDto>.Failure("COURSE_NOT_FOUND", "Course not found.");

        // Prevent simultaneous duplicate active/pending enrollments for the same student+course
        var alreadyEnrolled = await _repository.ActiveEnrollmentExistsAsync(
            _tenant.TenantId, request.StudentId, request.CourseId, ct: ct);
        if (alreadyEnrolled)
            return Result<StudentCourseDto>.Failure(
                "STUDENT_COURSE_DUPLICATE_ACTIVE",
                "This student already has an active or pending enrollment in this course.");

        var enrollment = StudentCourse.Create(
            tenantId:           _tenant.TenantId,
            studentId:          request.StudentId,
            courseId:           request.CourseId,
            enrollmentDate:     request.EnrollmentDate,
            startDate:          request.StartDate,
            endDate:            request.EndDate,
            classGroup:         request.ClassGroup,
            shift:              request.Shift,
            scheduleDescription: request.ScheduleDescription,
            unitId:             request.UnitId,
            notes:              request.Notes);

        await _repository.AddAsync(enrollment, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<StudentCourseDto>.Success(enrollment.Adapt<StudentCourseDto>());
    }
}
