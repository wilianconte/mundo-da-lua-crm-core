using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetCourseById;

public sealed class GetCourseByIdHandler : IRequestHandler<GetCourseByIdQuery, Result<CourseDto>>
{
    private readonly ICourseRepository _repository;

    public GetCourseByIdHandler(ICourseRepository repository) => _repository = repository;

    public async Task<Result<CourseDto>> Handle(GetCourseByIdQuery request, CancellationToken ct)
    {
        var course = await _repository.GetByIdAsync(request.Id, ct);

        if (course is null)
            return Result<CourseDto>.Failure("COURSE_NOT_FOUND", "Course not found.");

        return Result<CourseDto>.Success(course.Adapt<CourseDto>());
    }
}
