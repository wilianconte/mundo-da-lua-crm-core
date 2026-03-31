using MyCRM.CRM.Application.Commands.Customers.ActivateCustomer;
using MyCRM.CRM.Application.Commands.Customers.CreateCustomer;
using MyCRM.CRM.Application.Commands.Customers.DeactivateCustomer;
using MyCRM.CRM.Application.Commands.Customers.DeleteCustomer;
using MyCRM.CRM.Application.Commands.Customers.SetCustomerAddress;
using MyCRM.CRM.Application.Commands.Customers.UpdateCustomer;
using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.GraphQL.GraphQL.Customers.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Customers;

[MutationType]
public class CustomerMutations
{
    [Authorize(Policy = SystemPermissions.CustomersCreate)]
    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerInput input, [Service] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCustomerCommand(input.Name, input.Email, input.Type, input.Phone, input.Document), ct);
        return result.IsSuccess ? result.Value! : throw new GraphQLException(result.Errors.Select(e => ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.CustomersUpdate)]
    public async Task<CustomerDto> UpdateCustomerAsync(UpdateCustomerInput input, [Service] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateCustomerCommand(input.Id, input.Name, input.Email, input.Phone, input.Document, input.Notes), ct);
        return result.IsSuccess ? result.Value! : throw new GraphQLException(result.Errors.Select(e => ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.CustomersUpdate)]
    public async Task<CustomerDto> SetCustomerAddressAsync(SetCustomerAddressInput input, [Service] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetCustomerAddressCommand(input.CustomerId, input.Street, input.Number, input.Complement, input.Neighborhood, input.City, input.State, input.ZipCode, input.Country), ct);
        return result.IsSuccess ? result.Value! : throw new GraphQLException(result.Errors.Select(e => ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.CustomersUpdate)]
    public async Task<bool> ActivateCustomerAsync(Guid id, [Service] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivateCustomerCommand(id), ct);
        return result.IsSuccess ? true : throw new GraphQLException(result.Errors.Select(e => ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.CustomersUpdate)]
    public async Task<bool> DeactivateCustomerAsync(Guid id, [Service] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateCustomerCommand(id), ct);
        return result.IsSuccess ? true : throw new GraphQLException(result.Errors.Select(e => ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.CustomersDelete)]
    public async Task<bool> DeleteCustomerAsync(Guid id, [Service] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteCustomerCommand(id), ct);
        return result.IsSuccess ? true : throw new GraphQLException(result.Errors.Select(e => ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}
