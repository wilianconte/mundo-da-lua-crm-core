using MediatR;
using MyCRM.Auth.Application.Commands.Users.CreateUser;
using MyCRM.Auth.Application.Commands.Login;
using MyCRM.Auth.Application.DTOs;
using MyCRM.GraphQL.GraphQL.Auth.Inputs;

namespace MyCRM.GraphQL.GraphQL.Auth;

[MutationType]
public class AuthMutations
{
    [Authorize]
    public async Task<UserDto> CreateUserAsync(
        CreateUserInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateUserCommand(
            input.Name,
            input.Email,
            input.Password,
            input.PersonId), ct);

        return result.IsSuccess
            ? result.Value!
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
}
