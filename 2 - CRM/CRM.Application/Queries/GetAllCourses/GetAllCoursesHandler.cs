using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllCourses;

public sealed class GetAllCoursesHandler : IRequestHandler<GetAllCoursesQuery, Result<IReadOnlyList<CourseDto>>>
{
    private readonly ICourseRepository _repository;

    public GetAllCoursesHandler(ICourseRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<CourseDto>>> Handle(GetAllCoursesQuery request, CancellationToken ct)
    {
        var courses = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<CourseDto>>.Success(courses.Adapt<IReadOnlyList<CourseDto>>());
    }
}
