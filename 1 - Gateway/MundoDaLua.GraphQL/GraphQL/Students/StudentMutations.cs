using HotChocolate;
using HotChocolate.Types;
using MediatR;
using MyCRM.CRM.Application.Commands.Students.CreateStudent;
using MyCRM.CRM.Application.Commands.Students.UpdateStudent;
using MyCRM.CRM.Application.Commands.Students.DeleteStudent;
using MyCRM.GraphQL.GraphQL.Students.Inputs;

namespace MyCRM.GraphQL.GraphQL.Students;

[Authorize]
[MutationType]
public sealed class StudentMutations
{
    public async Task<StudentPayload> CreateStudentAsync(
        CreateStudentInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateStudentCommand(
            input.PersonId,
            input.RegistrationNumber,
            input.SchoolName,
            input.GradeOrClass,
            input.EnrollmentType,
            input.UnitId,
            input.ClassGroup,
            input.StartDate,
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

    public async Task<StudentPayload> UpdateStudentAsync(
        Guid id,
        UpdateStudentInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateStudentCommand(
            id,
            input.RegistrationNumber,
            input.SchoolName,
            input.GradeOrClass,
            input.EnrollmentType,
            input.UnitId,
            input.ClassGroup,
            input.StartDate,
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
