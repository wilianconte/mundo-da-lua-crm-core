using MediatR;
using MyCRM.Auth.Application.Commands.Users.CreateUser;
using MyCRM.Auth.Application.Commands.Users.DeleteUser;
using MyCRM.Auth.Application.Commands.Users.UpdateUser;
using MyCRM.Auth.Application.Commands.Login;
using MyCRM.Auth.Application.Commands.LoginByEmail;
using MyCRM.Auth.Application.Commands.RefreshToken;
using MyCRM.Auth.Application.Commands.Auth.RequestPasswordReset;
using MyCRM.Auth.Application.Commands.Auth.ResetPassword;
using MyCRM.Auth.Application.DTOs;
using MyCRM.GraphQL.GraphQL.Auth.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Auth;

[MutationType]
public class AuthMutations
{
    [Authorize(Policy = SystemPermissions.UsersManage)]
    public async Task<UserDto> CreateUserAsync(
        CreateUserInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateUserCommand(
            input.Name,
            input.Email,
            input.Password,
            input.PersonId,
            input.RoleIds), ct);

        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.UsersManage)]
    public async Task<UserDto> UpdateUserAsync(
        Guid id,
        UpdateUserInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateUserCommand(
            id,
            input.Name,
            input.Email,
            input.PersonId,
            input.IsActive,
            input.IsAdmin,
            input.Password,
            input.RoleIds), ct);

        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.UsersManage)]
    public async Task<bool> DeleteUserAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteUserCommand(id), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [AllowAnonymous]
    public async Task<LoginDto> LoginAsync(LoginInput input, [Service] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(input.TenantId, input.Email, input.Password), ct);
        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    /// <summary>
    /// Autentica o usuário usando apenas email e senha, resolvendo o tenantId automaticamente.
    /// Útil quando o cliente não possui o tenantId previamente.
    /// </summary>
    [AllowAnonymous]
    public async Task<LoginDto> LoginByEmailAsync(LoginByEmailInput input, [Service] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginByEmailCommand(input.Email, input.Password), ct);
        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [AllowAnonymous]
    public async Task<LoginDto> RefreshTokenAsync(RefreshTokenInput input, [Service] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(input.TenantId, input.RefreshToken), ct);
        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [AllowAnonymous]
    public async Task<bool> RequestPasswordResetAsync(
        RequestPasswordResetInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new RequestPasswordResetCommand(input.Email), ct);
        return result.IsSuccess
            ? result.Value
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [AllowAnonymous]
    public async Task<bool> ResetPasswordAsync(
        ResetPasswordInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ResetPasswordCommand(input.Token, input.NewPassword, input.NewPasswordConfirmation), ct);
        return result.IsSuccess
            ? result.Value
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}
