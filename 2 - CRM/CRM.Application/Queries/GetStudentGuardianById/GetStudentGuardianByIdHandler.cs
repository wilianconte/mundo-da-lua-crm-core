using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetStudentGuardianById;

public sealed class GetStudentGuardianByIdHandler : IRequestHandler<GetStudentGuardianByIdQuery, Result<StudentGuardianDto>>
{
    private readonly IStudentGuardianRepository _repository;

    public GetStudentGuardianByIdHandler(IStudentGuardianRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<StudentGuardianDto>> Handle(GetStudentGuardianByIdQuery request, CancellationToken ct)
    {
        var guardian = await _repository.GetByIdAsync(request.Id, ct);

        if (guardian is null)
            return Result<StudentGuardianDto>.Failure("STUDENT_GUARDIAN_NOT_FOUND", "Student guardian not found.");

        return Result<StudentGuardianDto>.Success(guardian.Adapt<StudentGuardianDto>());
    }
}
