using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetStudentCourseById;

public sealed class GetStudentCourseByIdHandler : IRequestHandler<GetStudentCourseByIdQuery, Result<StudentCourseDto>>
{
    private readonly IStudentCourseRepository _repository;

    public GetStudentCourseByIdHandler(IStudentCourseRepository repository) => _repository = repository;

    public async Task<Result<StudentCourseDto>> Handle(GetStudentCourseByIdQuery request, CancellationToken ct)
    {
        var enrollment = await _repository.GetByIdAsync(request.Id, ct);

        if (enrollment is null)
            return Result<StudentCourseDto>.Failure("STUDENT_COURSE_NOT_FOUND", "Enrollment not found.");

        return Result<StudentCourseDto>.Success(enrollment.Adapt<StudentCourseDto>());
    }
}
