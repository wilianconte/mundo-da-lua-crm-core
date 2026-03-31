using HotChocolate;
using HotChocolate.Types;
using MediatR;
using MyCRM.CRM.Application.Commands.StudentCourses.CreateStudentCourse;
using MyCRM.CRM.Application.Commands.StudentCourses.UpdateStudentCourse;
using MyCRM.CRM.Application.Commands.StudentCourses.DeleteStudentCourse;
using MyCRM.GraphQL.GraphQL.StudentCourses.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.StudentCourses;

[MutationType]
public sealed class StudentCourseMutations
{
    [Authorize(Policy = SystemPermissions.StudentCoursesCreate)]
    public async Task<StudentCoursePayload> CreateStudentCourseAsync(
        CreateStudentCourseInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateStudentCourseCommand(
            input.StudentId,
            input.CourseId,
            input.EnrollmentDate,
            input.StartDate,
            input.EndDate,
            input.ClassGroup,
            input.Shift,
            input.ScheduleDescription,
            input.UnitId,
            input.Notes), ct);

        return result.IsSuccess
            ? new StudentCoursePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    [Authorize(Policy = SystemPermissions.StudentCoursesUpdate)]
    public async Task<StudentCoursePayload> UpdateStudentCourseAsync(
        Guid id,
        UpdateStudentCourseInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateStudentCourseCommand(
            id,
            input.EnrollmentDate,
            input.StartDate,
            input.EndDate,
            input.ClassGroup,
            input.Shift,
            input.ScheduleDescription,
            input.UnitId,
            input.Notes,
            input.Status), ct);

        return result.IsSuccess
            ? new StudentCoursePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    [Authorize(Policy = SystemPermissions.StudentCoursesDelete)]
    public async Task<bool> DeleteStudentCourseAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteStudentCourseCommand(id), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }
}
