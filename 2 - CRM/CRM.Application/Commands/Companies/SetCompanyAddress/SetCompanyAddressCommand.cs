using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Companies.SetCompanyAddress;

public record SetCompanyAddressCommand(
    Guid CompanyId,
    string Street,
    string? Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country = "BR"
) : IRequest<Result<CompanyDto>>;
