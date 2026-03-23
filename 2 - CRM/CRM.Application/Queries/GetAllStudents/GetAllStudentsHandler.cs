using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllStudents;

public sealed class GetAllStudentsHandler : IRequestHandler<GetAllStudentsQuery, Result<IReadOnlyList<StudentDto>>>
{
    private readonly IStudentRepository _repository;

    public GetAllStudentsHandler(IStudentRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<StudentDto>>> Handle(GetAllStudentsQuery request, CancellationToken ct)
    {
        var students = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<StudentDto>>.Success(students.Adapt<IReadOnlyList<StudentDto>>());
    }
}
