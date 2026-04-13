using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.LoginByEmail;

public record LoginByEmailCommand(string Email, string Password) : IRequest<Result<LoginDto>>;
