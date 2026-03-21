using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Login;

public record LoginCommand(Guid TenantId, string Email, string Password) : IRequest<Result<LoginDto>>;
