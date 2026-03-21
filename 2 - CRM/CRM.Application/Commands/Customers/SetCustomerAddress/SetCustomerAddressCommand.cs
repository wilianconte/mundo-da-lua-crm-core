using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Customers.SetCustomerAddress;

public record SetCustomerAddressCommand(Guid Id, string Street, string? Number, string? Complement, string Neighborhood, string City, string State, string ZipCode, string Country = "BR") : IRequest<Result<CustomerDto>>;
