using MyCRM.CRM.Domain.Repositories;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentGuardians.DeleteStudentGuardian;

public sealed class DeleteStudentGuardianHandler : IRequestHandler<DeleteStudentGuardianCommand, Result>
{
    private readonly IStudentGuardianRepository _repository;

    public DeleteStudentGuardianHandler(IStudentGuardianRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteStudentGuardianCommand request, CancellationToken ct)
    {
        var guardian = await _repository.GetByIdAsync(request.Id, ct);
        if (guardian is null)
            return Result.Failure("STUDENT_GUARDIAN_NOT_FOUND", "Student guardian not found.");

        _repository.Delete(guardian);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
