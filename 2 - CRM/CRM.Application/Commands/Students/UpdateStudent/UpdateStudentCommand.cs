using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Students.UpdateStudent;

public record UpdateStudentCommand(
    Guid Id,
    Guid? UnitId,
    string? Notes
) : IRequest<Result<StudentDto>>;
