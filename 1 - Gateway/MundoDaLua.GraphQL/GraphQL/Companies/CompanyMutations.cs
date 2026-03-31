using MyCRM.CRM.Application.Commands.Companies.CreateCompany;
using MyCRM.CRM.Application.Commands.Companies.DeleteCompany;
using MyCRM.CRM.Application.Commands.Companies.SetCompanyAddress;
using MyCRM.CRM.Application.Commands.Companies.UpdateCompany;
using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.GraphQL.GraphQL.Companies.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Companies;

[MutationType]
public sealed class CompanyMutations
{
    [Authorize(Policy = SystemPermissions.CompaniesCreate)]
    public async Task<CompanyDto> CreateCompanyAsync(
        CreateCompanyInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateCompanyCommand(
            input.LegalName,
            input.TradeName,
            input.RegistrationNumber,
            input.StateRegistration,
            input.MunicipalRegistration,
            input.Email,
            input.PrimaryPhone,
            input.SecondaryPhone,
            input.WhatsAppNumber,
            input.Website,
            input.ContactPersonName,
            input.ContactPersonEmail,
            input.ContactPersonPhone,
            input.CompanyType,
            input.Industry,
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

    [Authorize(Policy = SystemPermissions.CompaniesUpdate)]
    public async Task<CompanyDto> UpdateCompanyAsync(
        Guid id,
        UpdateCompanyInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateCompanyCommand(
            id,
            input.LegalName,
            input.TradeName,
            input.RegistrationNumber,
            input.StateRegistration,
            input.MunicipalRegistration,
            input.Email,
            input.PrimaryPhone,
            input.SecondaryPhone,
            input.WhatsAppNumber,
            input.Website,
            input.ContactPersonName,
            input.ContactPersonEmail,
            input.ContactPersonPhone,
            input.CompanyType,
            input.Industry,
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

    [Authorize(Policy = SystemPermissions.CompaniesUpdate)]
    public async Task<CompanyDto> SetCompanyAddressAsync(
        SetCompanyAddressInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new SetCompanyAddressCommand(
            input.CompanyId,
            input.Street,
            input.Number,
            input.Complement,
            input.Neighborhood,
            input.City,
            input.State,
            input.ZipCode,
            input.Country), ct);

        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    [Authorize(Policy = SystemPermissions.CompaniesDelete)]
    public async Task<bool> DeleteCompanyAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteCompanyCommand(id), ct);

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
