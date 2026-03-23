using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Courses.DeleteCourse;

public record DeleteCourseCommand(Guid Id) : IRequest<Result<bool>>;
