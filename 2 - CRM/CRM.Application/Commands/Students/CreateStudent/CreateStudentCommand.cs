using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Students.CreateStudent;

public record CreateStudentCommand(
    Guid PersonId,
    string? RegistrationNumber,
    string? SchoolName,
    string? GradeOrClass,
    string? EnrollmentType,
    Guid? UnitId,
    string? ClassGroup,
    DateOnly? StartDate,
    string? Notes
) : IRequest<Result<StudentDto>>;
