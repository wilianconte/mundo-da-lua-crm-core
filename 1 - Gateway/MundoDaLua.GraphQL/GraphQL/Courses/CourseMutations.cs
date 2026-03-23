using HotChocolate;
using HotChocolate.Types;
using MediatR;
using MyCRM.CRM.Application.Commands.Courses.CreateCourse;
using MyCRM.CRM.Application.Commands.Courses.UpdateCourse;
using MyCRM.CRM.Application.Commands.Courses.DeleteCourse;
using MyCRM.GraphQL.GraphQL.Courses.Inputs;

namespace MyCRM.GraphQL.GraphQL.Courses;

[MutationType]
public sealed class CourseMutations
{
    public async Task<CoursePayload> CreateCourseAsync(
        CreateCourseInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateCourseCommand(
            input.Name,
            input.Type,
            input.Code,
            input.Description,
            input.StartDate,
            input.EndDate,
            input.ScheduleDescription,
            input.Capacity,
            input.Workload,
            input.UnitId,
            input.Notes,
            input.Status), ct);

        return result.IsSuccess
            ? new CoursePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    public async Task<CoursePayload> UpdateCourseAsync(
        Guid id,
        UpdateCourseInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateCourseCommand(
            id,
            input.Name,
            input.Type,
            input.Code,
            input.Description,
            input.StartDate,
            input.EndDate,
            input.ScheduleDescription,
            input.Capacity,
            input.Workload,
            input.UnitId,
            input.Notes,
            input.Status), ct);

        return result.IsSuccess
            ? new CoursePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    public async Task<bool> DeleteCourseAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteCourseCommand(id), ct);

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
