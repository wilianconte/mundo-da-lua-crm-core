using MyCRM.CRM.Domain.Repositories;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Students.DeleteStudent;

public sealed class DeleteStudentHandler : IRequestHandler<DeleteStudentCommand, Result>
{
    private readonly IStudentRepository _repository;

    public DeleteStudentHandler(IStudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteStudentCommand request, CancellationToken ct)
    {
        var student = await _repository.GetByIdAsync(request.Id, ct);
        if (student is null)
            return Result.Failure("STUDENT_NOT_FOUND", "Student not found.");

        _repository.Delete(student);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
