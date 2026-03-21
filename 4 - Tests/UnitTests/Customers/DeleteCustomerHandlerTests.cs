using MyCRM.CRM.Application.Commands.DeleteCustomer;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using NSubstitute;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.UnitTests.Customers;

public sealed class DeleteCustomerHandlerTests
{
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly ITenantService _tenant = Substitute.For<ITenantService>();
    private readonly DeleteCustomerHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public DeleteCustomerHandlerTests()
    {
        _tenant.TenantId.Returns(_tenantId);
        _handler = new DeleteCustomerHandler(_repository, _tenant);
    }

    [Fact]
    public async Task Handle_ExistingCustomer_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(_tenantId, "John Doe", "john@example.com", CustomerType.Individual);

        _repository.GetByIdAsync(customerId, default).Returns(customer);
        _repository.SaveChangesAsync(default).Returns(1);

        var result = await _handler.Handle(new DeleteCustomerCommand(customerId), default);

        Assert.True(result.IsSuccess);
        _repository.Received(1).Delete(customer);
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ReturnsFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Customer?)null);

        var result = await _handler.Handle(new DeleteCustomerCommand(Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("CUSTOMER_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_CustomerFromDifferentTenant_ReturnsFailure()
    {
        var customerId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var customer = Customer.Create(otherTenantId, "Jane Doe", "jane@example.com", CustomerType.Individual);

        _repository.GetByIdAsync(customerId, default).Returns(customer);

        var result = await _handler.Handle(new DeleteCustomerCommand(customerId), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("CUSTOMER_NOT_FOUND", result.ErrorCode);
        _repository.DidNotReceive().Delete(Arg.Any<Customer>());
    }
}
