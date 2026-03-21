using MyCRM.Customers.Application.DTOs;
using MyCRM.Customers.Domain.Entities;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Customers.Application.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string Email,
    CustomerType Type,
    string? Phone,
    string? Document
) : IRequest<Result<CustomerDto>>;
