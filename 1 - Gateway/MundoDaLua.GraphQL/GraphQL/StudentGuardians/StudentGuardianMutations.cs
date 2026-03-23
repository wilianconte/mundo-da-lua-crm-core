using HotChocolate;
using HotChocolate.Types;
using MediatR;
using MyCRM.CRM.Application.Commands.StudentGuardians.CreateStudentGuardian;
using MyCRM.CRM.Application.Commands.StudentGuardians.UpdateStudentGuardian;
using MyCRM.CRM.Application.Commands.StudentGuardians.DeleteStudentGuardian;
using MyCRM.GraphQL.GraphQL.StudentGuardians.Inputs;

namespace MyCRM.GraphQL.GraphQL.StudentGuardians;

[MutationType]
public sealed class StudentGuardianMutations
{
    public async Task<StudentGuardianPayload> CreateStudentGuardianAsync(
        CreateStudentGuardianInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateStudentGuardianCommand(
            input.StudentId,
            input.GuardianPersonId,
            input.RelationshipType,
            input.IsPrimaryGuardian,
            input.IsFinancialResponsible,
            input.ReceivesNotifications,
            input.CanPickupChild,
            input.Notes), ct);

        return result.IsSuccess
            ? new StudentGuardianPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    public async Task<StudentGuardianPayload> UpdateStudentGuardianAsync(
        Guid id,
        UpdateStudentGuardianInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateStudentGuardianCommand(
            id,
            input.RelationshipType,
            input.IsPrimaryGuardian,
            input.IsFinancialResponsible,
            input.ReceivesNotifications,
            input.CanPickupChild,
            input.Notes), ct);

        return result.IsSuccess
            ? new StudentGuardianPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    public async Task<bool> DeleteStudentGuardianAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteStudentGuardianCommand(id), ct);

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
