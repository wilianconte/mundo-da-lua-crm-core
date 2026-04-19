using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Wallets.DeleteWallet;

public record DeleteWalletCommand(Guid Id) : IRequest<Result>;
