using MyCRM.CRM.Application.Commands.UpdateCustomer;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using NSubstitute;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.UnitTests.Customers;

public sealed class UpdateCustomerHandlerTests
{
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly ITenantService _tenant = Substitute.For<ITenantService>();
    private readonly UpdateCustomerHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UpdateCustomerHandlerTests()
    {
        _tenant.TenantId.Returns(_tenantId);
        _handler = new UpdateCustomerHandler(_repository, _tenant);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(_tenantId, "Old Name", "old@example.com", CustomerType.Individual);

        _repository.GetByIdAsync(customerId, default).Returns(customer);
        _repository.EmailExistsAsync(_tenantId, "new@example.com", customerId, default).Returns(false);
        _repository.SaveChangesAsync(default).Returns(1);

        var command = new UpdateCustomerCommand(
            Id: customerId,
            Name: "New Name",
            Email: "new@example.com",
            Phone: null,
            Document: null,
            Notes: "Some notes");

        var result = await _handler.Handle(command, default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("New Name", result.Value.Name);
        Assert.Equal("new@example.com", result.Value.Email);
        Assert.Equal("Some notes", result.Value.Notes);
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ReturnsFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Customer?)null);

        var command = new UpdateCustomerCommand(
            Id: Guid.NewGuid(),
            Name: "Name",
            Email: "email@example.com",
            Phone: null,
            Document: null,
            Notes: null);

        var result = await _handler.Handle(command, default);

        Assert.False(result.IsSuccess);
        Assert.Equal("CUSTOMER_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(_tenantId, "Name", "original@example.com", CustomerType.Individual);

        _repository.GetByIdAsync(customerId, default).Returns(customer);
        _repository.EmailExistsAsync(_tenantId, "taken@example.com", customerId, default).Returns(true);

        var command = new UpdateCustomerCommand(
            Id: customerId,
            Name: "Name",
            Email: "taken@example.com",
            Phone: null,
            Document: null,
            Notes: null);

        var result = await _handler.Handle(command, default);

        Assert.False(result.IsSuccess);
        Assert.Equal("CUSTOMER_EMAIL_DUPLICATE", result.ErrorCode);
    }
}
