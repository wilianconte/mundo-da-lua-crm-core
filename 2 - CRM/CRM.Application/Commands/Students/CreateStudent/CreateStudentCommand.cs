using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Students.CreateStudent;

public record CreateStudentCommand(
    Guid PersonId,
    Guid? UnitId,
    string? Notes
) : IRequest<Result<StudentDto>>;
