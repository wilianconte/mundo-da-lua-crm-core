using MediatR;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.PaymentMethods.DeletePaymentMethod;

public sealed class DeletePaymentMethodHandler : IRequestHandler<DeletePaymentMethodCommand, Result>
{
    private readonly IPaymentMethodRepository _repository;

    public DeletePaymentMethodHandler(IPaymentMethodRepository repository) => _repository = repository;

    public async Task<Result> Handle(DeletePaymentMethodCommand request, CancellationToken ct)
    {
        var method = await _repository.GetByIdAsync(request.Id, ct);
        if (method is null)
            return Result.Failure("PAYMENT_METHOD_NOT_FOUND", "Payment method not found.");

        method.SoftDelete();
        _repository.Update(method);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
