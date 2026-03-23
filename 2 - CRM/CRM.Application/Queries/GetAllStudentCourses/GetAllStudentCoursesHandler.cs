using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllStudentCourses;

public sealed class GetAllStudentCoursesHandler : IRequestHandler<GetAllStudentCoursesQuery, Result<IReadOnlyList<StudentCourseDto>>>
{
    private readonly IStudentCourseRepository _repository;

    public GetAllStudentCoursesHandler(IStudentCourseRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<StudentCourseDto>>> Handle(GetAllStudentCoursesQuery request, CancellationToken ct)
    {
        var enrollments = await _repository.Query().ToListAsync(ct);
        return Result<IReadOnlyList<StudentCourseDto>>.Success(enrollments.Adapt<IReadOnlyList<StudentCourseDto>>());
    }
}
