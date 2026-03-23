using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Courses.UpdateCourse;

public sealed class UpdateCourseHandler : IRequestHandler<UpdateCourseCommand, Result<CourseDto>>
{
    private readonly ICourseRepository _repository;
    private readonly ITenantService    _tenant;

    public UpdateCourseHandler(ICourseRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant     = tenant;
    }

    public async Task<Result<CourseDto>> Handle(UpdateCourseCommand request, CancellationToken ct)
    {
        var course = await _repository.GetByIdAsync(request.Id, ct);
        if (course is null)
            return Result<CourseDto>.Failure("COURSE_NOT_FOUND", "Course not found.");

        // Prevent duplicate course codes on update
        if (request.Code is not null)
        {
            var codeExists = await _repository.CodeExistsAsync(
                _tenant.TenantId, request.Code, excludeId: request.Id, ct: ct);
            if (codeExists)
                return Result<CourseDto>.Failure(
                    "COURSE_CODE_DUPLICATE",
                    "A course with this code already exists.");
        }

        course.UpdateInfo(
            name:               request.Name,
            type:               request.Type,
            code:               request.Code,
            description:        request.Description,
            startDate:          request.StartDate,
            endDate:            request.EndDate,
            scheduleDescription: request.ScheduleDescription,
            capacity:           request.Capacity,
            workload:           request.Workload,
            unitId:             request.UnitId,
            notes:              request.Notes);

        _repository.Update(course);
        await _repository.SaveChangesAsync(ct);

        return Result<CourseDto>.Success(course.Adapt<CourseDto>());
    }
}
