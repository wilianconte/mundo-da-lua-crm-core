using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Wallets.UpdateWallet;

public record UpdateWalletCommand(Guid Id, string Name, decimal InitialBalance) : IRequest<Result<WalletDto>>;
