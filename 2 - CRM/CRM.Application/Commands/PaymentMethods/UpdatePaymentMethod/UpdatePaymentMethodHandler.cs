using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.PaymentMethods.UpdatePaymentMethod;

public sealed class UpdatePaymentMethodHandler : IRequestHandler<UpdatePaymentMethodCommand, Result<PaymentMethodDto>>
{
    private readonly IPaymentMethodRepository _repository;

    public UpdatePaymentMethodHandler(IPaymentMethodRepository repository) => _repository = repository;

    public async Task<Result<PaymentMethodDto>> Handle(UpdatePaymentMethodCommand request, CancellationToken ct)
    {
        var method = await _repository.GetByIdAsync(request.Id, ct);
        if (method is null)
            return Result<PaymentMethodDto>.Failure("PAYMENT_METHOD_NOT_FOUND", "Payment method not found.");

        method.Update(request.Name);
        _repository.Update(method);
        await _repository.SaveChangesAsync(ct);

        return Result<PaymentMethodDto>.Success(method.Adapt<PaymentMethodDto>());
    }
}
