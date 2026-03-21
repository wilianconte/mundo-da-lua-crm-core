using MyCRM.CRM.Application.Commands.Customers.CreateCustomer;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using NSubstitute;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.UnitTests.Customers;

public sealed class CreateCustomerHandlerTests
{
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly ITenantService _tenant = Substitute.For<ITenantService>();
    private readonly CreateCustomerHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateCustomerHandlerTests()
    {
        _tenant.TenantId.Returns(_tenantId);
        _handler = new CreateCustomerHandler(_repository, _tenant);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        _repository.EmailExistsAsync(_tenantId, "john@example.com", ct: default).Returns(false);
        _repository.SaveChangesAsync(default).Returns(1);

        var command = new CreateCustomerCommand("John Doe", "john@example.com", CustomerType.Individual, null, null);

        var result = await _handler.Handle(command, default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("John Doe", result.Value.Name);
        Assert.Equal("john@example.com", result.Value.Email);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        _repository.EmailExistsAsync(_tenantId, "existing@example.com", ct: default).Returns(true);

        var command = new CreateCustomerCommand("Jane Doe", "existing@example.com", CustomerType.Individual, null, null);

        var result = await _handler.Handle(command, default);

        Assert.False(result.IsSuccess);
        Assert.Equal("CUSTOMER_EMAIL_DUPLICATE", result.ErrorCode);
    }
}


