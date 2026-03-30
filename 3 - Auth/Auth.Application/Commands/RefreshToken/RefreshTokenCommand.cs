using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.RefreshToken;

public record RefreshTokenCommand(Guid TenantId, string RefreshToken) : IRequest<Result<LoginDto>>;
