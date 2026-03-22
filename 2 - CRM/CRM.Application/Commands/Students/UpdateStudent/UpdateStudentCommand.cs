using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Students.UpdateStudent;

public record UpdateStudentCommand(
    Guid Id,
    string? RegistrationNumber,
    string? SchoolName,
    string? GradeOrClass,
    string? EnrollmentType,
    Guid? UnitId,
    string? ClassGroup,
    DateOnly? StartDate,
    string? Notes,
    string? AcademicObservation
) : IRequest<Result<StudentDto>>;
