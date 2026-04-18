using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Wallets.CreateWallet;

public record CreateWalletCommand(string Name, decimal InitialBalance) : IRequest<Result<WalletDto>>;
