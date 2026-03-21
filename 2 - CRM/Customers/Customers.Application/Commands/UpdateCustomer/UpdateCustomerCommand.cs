using MyCRM.Customers.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Customers.Application.Commands.UpdateCustomer;

public record UpdateCustomerCommand(Guid Id, string Name, string Email, string? Phone, string? Document, string? Notes) : IRequest<Result<CustomerDto>>;
