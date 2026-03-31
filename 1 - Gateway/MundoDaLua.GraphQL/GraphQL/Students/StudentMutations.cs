using HotChocolate;
using HotChocolate.Types;
using MediatR;
using MyCRM.CRM.Application.Commands.Students.CreateStudent;
using MyCRM.CRM.Application.Commands.Students.UpdateStudent;
using MyCRM.CRM.Application.Commands.Students.DeleteStudent;
using MyCRM.GraphQL.GraphQL.Students.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Students;

[MutationType]
public sealed class StudentMutations
{
    [Authorize(Policy = SystemPermissions.StudentsCreate)]
    public async Task<StudentPayload> CreateStudentAsync(
        CreateStudentInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateStudentCommand(
            input.PersonId,
            input.UnitId,
            input.Notes), ct);

        return result.IsSuccess
            ? new StudentPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    [Authorize(Policy = SystemPermissions.StudentsUpdate)]
    public async Task<StudentPayload> UpdateStudentAsync(
        Guid id,
        UpdateStudentInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateStudentCommand(
            id,
            input.UnitId,
            input.Notes), ct);

        return result.IsSuccess
            ? new StudentPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    [Authorize(Policy = SystemPermissions.StudentsDelete)]
    public async Task<bool> DeleteStudentAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteStudentCommand(id), ct);

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
