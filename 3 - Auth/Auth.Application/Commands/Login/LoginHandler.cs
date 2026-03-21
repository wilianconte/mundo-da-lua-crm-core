using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<LoginDto>>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokenGen;

    public LoginHandler(IUserRepository repo, IPasswordHasher hasher, ITokenGenerator tokenGen)
    {
        _repo = repo;
        _hasher = hasher;
        _tokenGen = tokenGen;
    }

    public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _repo.GetByEmailAsync(request.TenantId, request.Email, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginDto>.Failure("INVALID_CREDENTIALS", "Email ou senha inválidos.");

        if (!user.IsActive)
            return Result<LoginDto>.Failure("USER_INACTIVE", "Usuário inativo.");

        var (token, expiresAt) = _tokenGen.Generate(user);
        return Result<LoginDto>.Success(new LoginDto(token, expiresAt, user.Id, user.Name, user.Email));
    }
}
