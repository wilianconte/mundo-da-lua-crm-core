using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Students.UpdateStudent;

public sealed class UpdateStudentHandler : IRequestHandler<UpdateStudentCommand, Result<StudentDto>>
{
    private readonly IStudentRepository _repository;

    public UpdateStudentHandler(IStudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<StudentDto>> Handle(UpdateStudentCommand request, CancellationToken ct)
    {
        var student = await _repository.GetByIdAsync(request.Id, ct);
        if (student is null)
            return Result<StudentDto>.Failure("STUDENT_NOT_FOUND", "Student not found.");

        student.UpdateInfo(
            unitId: request.UnitId,
            notes:  request.Notes);

        _repository.Update(student);
        await _repository.SaveChangesAsync(ct);

        return Result<StudentDto>.Success(student.Adapt<StudentDto>());
    }
}
