using MyCRM.Application.DTOs;
using MyCRM.Domain.Entities;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Application.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string Email,
    CustomerType Type,
    string? Phone,
    string? Document
) : IRequest<Result<CustomerDto>>;
