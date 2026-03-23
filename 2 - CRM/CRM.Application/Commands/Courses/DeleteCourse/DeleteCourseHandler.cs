using MyCRM.CRM.Domain.Repositories;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Courses.DeleteCourse;

public sealed class DeleteCourseHandler : IRequestHandler<DeleteCourseCommand, Result<bool>>
{
    private readonly ICourseRepository _repository;

    public DeleteCourseHandler(ICourseRepository repository) => _repository = repository;

    public async Task<Result<bool>> Handle(DeleteCourseCommand request, CancellationToken ct)
    {
        var course = await _repository.GetByIdAsync(request.Id, ct);
        if (course is null)
            return Result<bool>.Failure("COURSE_NOT_FOUND", "Course not found.");

        _repository.Delete(course);
        await _repository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
