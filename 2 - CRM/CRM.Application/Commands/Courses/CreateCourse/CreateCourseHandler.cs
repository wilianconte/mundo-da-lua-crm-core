using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Exceptions;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;
using MyCRM.Shared.Kernel.Services;

namespace MyCRM.CRM.Application.Commands.Courses.CreateCourse;

public sealed class CreateCourseHandler : IRequestHandler<CreateCourseCommand, Result<CourseDto>>
{
    private readonly ICourseRepository _repository;
    private readonly ITenantService    _tenant;
    private readonly IPlanLimitService _planLimit;

    public CreateCourseHandler(ICourseRepository repository, ITenantService tenant, IPlanLimitService planLimit)
    {
        _repository = repository;
        _tenant     = tenant;
        _planLimit  = planLimit;
    }

    public async Task<Result<CourseDto>> Handle(CreateCourseCommand request, CancellationToken ct)
    {
        // Verifica limite de cursos do plano
        try
        {
            await _planLimit.CheckNumericLimitAsync(
                _tenant.TenantId,
                "max_courses",
                () => _repository.CountActiveByTenantAsync(_tenant.TenantId, ct),
                ct);
        }
        catch (PlanLimitException ex)
        {
            return Result<CourseDto>.Failure(ex.ErrorCode, ex.Message);
        }

        // Prevent duplicate course codes within the tenant
        if (request.Code is not null)
        {
            var codeExists = await _repository.CodeExistsAsync(
                _tenant.TenantId, request.Code, ct: ct);
            if (codeExists)
                return Result<CourseDto>.Failure(
                    "COURSE_CODE_DUPLICATE",
                    "A course with this code already exists.");
        }

        var course = Course.Create(
            tenantId:           _tenant.TenantId,
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
            notes:              request.Notes,
            status:             request.Status);

        await _repository.AddAsync(course, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<CourseDto>.Success(course.Adapt<CourseDto>());
    }
}
