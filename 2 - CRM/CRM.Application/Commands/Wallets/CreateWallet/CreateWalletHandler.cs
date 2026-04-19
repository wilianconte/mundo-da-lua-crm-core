using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Wallets.CreateWallet;

public sealed class CreateWalletHandler : IRequestHandler<CreateWalletCommand, Result<WalletDto>>
{
    private readonly IWalletRepository _repository;
    private readonly ITenantService    _tenant;

    public CreateWalletHandler(IWalletRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant     = tenant;
    }

    public async Task<Result<WalletDto>> Handle(CreateWalletCommand request, CancellationToken ct)
    {
        var wallet = Wallet.Create(_tenant.TenantId, request.Name, request.InitialBalance);

        await _repository.AddAsync(wallet, ct);
        await _repository.SaveChangesAsync(ct);

        var dto = wallet.Adapt<WalletDto>() with { Balance = wallet.InitialBalance };
        return Result<WalletDto>.Success(dto);
    }
}
