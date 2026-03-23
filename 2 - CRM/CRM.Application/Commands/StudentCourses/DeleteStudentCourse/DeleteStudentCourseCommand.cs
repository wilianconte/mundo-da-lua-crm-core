using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentCourses.DeleteStudentCourse;

public record DeleteStudentCourseCommand(Guid Id) : IRequest<Result<bool>>;
