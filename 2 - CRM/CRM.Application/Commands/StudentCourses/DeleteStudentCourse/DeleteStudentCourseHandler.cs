using MyCRM.CRM.Domain.Repositories;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentCourses.DeleteStudentCourse;

public sealed class DeleteStudentCourseHandler : IRequestHandler<DeleteStudentCourseCommand, Result<bool>>
{
    private readonly IStudentCourseRepository _repository;

    public DeleteStudentCourseHandler(IStudentCourseRepository repository) => _repository = repository;

    public async Task<Result<bool>> Handle(DeleteStudentCourseCommand request, CancellationToken ct)
    {
        var enrollment = await _repository.GetByIdAsync(request.Id, ct);
        if (enrollment is null)
            return Result<bool>.Failure("STUDENT_COURSE_NOT_FOUND", "Enrollment not found.");

        _repository.Delete(enrollment);
        await _repository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
