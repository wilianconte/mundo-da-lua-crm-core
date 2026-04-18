using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllWallets;

public record GetAllWalletsQuery : IRequest<Result<IReadOnlyList<WalletDto>>>;
