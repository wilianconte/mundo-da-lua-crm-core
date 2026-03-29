using MyCRM.CRM.Application.Commands.People.CreatePerson;
using MyCRM.CRM.Application.Commands.People.DeletePerson;
using MyCRM.CRM.Application.Commands.People.UpdatePerson;
using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.GraphQL.GraphQL.People.Inputs;

namespace MyCRM.GraphQL.GraphQL.People;

[Authorize]
[MutationType]
public sealed class PersonMutations
{
    public async Task<PersonDto> CreatePersonAsync(
        CreatePersonInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreatePersonCommand(
            input.FullName,
            input.PreferredName,
            input.DocumentNumber,
            input.BirthDate,
            input.Gender,
            input.MaritalStatus,
            input.Nationality,
            input.Occupation,
            input.Email,
            input.PrimaryPhone,
            input.SecondaryPhone,
            input.WhatsAppNumber,
            input.ProfileImageUrl,
            input.Notes), ct);

        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    public async Task<PersonDto> UpdatePersonAsync(
        Guid id,
        UpdatePersonInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdatePersonCommand(
            id,
            input.FullName,
            input.PreferredName,
            input.DocumentNumber,
            input.BirthDate,
            input.Gender,
            input.MaritalStatus,
            input.Nationality,
            input.Occupation,
            input.Email,
            input.PrimaryPhone,
            input.SecondaryPhone,
            input.WhatsAppNumber,
            input.ProfileImageUrl,
            input.Notes), ct);

        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    public async Task<bool> DeletePersonAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeletePersonCommand(id), ct);

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

