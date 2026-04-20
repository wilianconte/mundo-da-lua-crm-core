using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.PaymentMethods.CreatePaymentMethod;

public sealed class CreatePaymentMethodHandler : IRequestHandler<CreatePaymentMethodCommand, Result<PaymentMethodDto>>
{
    private readonly IPaymentMethodRepository _repository;
    private readonly ITenantService           _tenant;

    public CreatePaymentMethodHandler(IPaymentMethodRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant     = tenant;
    }

    public async Task<Result<PaymentMethodDto>> Handle(CreatePaymentMethodCommand request, CancellationToken ct)
    {
        var method = PaymentMethod.Create(_tenant.TenantId, request.Name, request.WalletId);
        await _repository.AddAsync(method, ct);
        await _repository.SaveChangesAsync(ct);
        return Result<PaymentMethodDto>.Success(method.Adapt<PaymentMethodDto>());
    }
}
