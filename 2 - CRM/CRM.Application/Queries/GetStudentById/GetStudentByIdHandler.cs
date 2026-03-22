using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetStudentById;

public sealed class GetStudentByIdHandler : IRequestHandler<GetStudentByIdQuery, Result<StudentDto>>
{
    private readonly IStudentRepository _repository;

    public GetStudentByIdHandler(IStudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<StudentDto>> Handle(GetStudentByIdQuery request, CancellationToken ct)
    {
        var student = await _repository.GetByIdAsync(request.Id, ct);

        if (student is null)
            return Result<StudentDto>.Failure("STUDENT_NOT_FOUND", "Student not found.");

        return Result<StudentDto>.Success(student.Adapt<StudentDto>());
    }
}
