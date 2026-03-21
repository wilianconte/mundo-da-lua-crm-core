using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string Email,
    CustomerType Type,
    string? Phone,
    string? Document
) : IRequest<Result<CustomerDto>>;
