using System.Security.Claims;
using HotChocolate.Execution;
using HotChocolate.Types;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.Auth.Application.Commands.Login;
using MyCRM.Auth.Application.Commands.RefreshToken;
using MyCRM.Auth.Application.Commands.Users.CreateUser;
using MyCRM.Auth.Application.Commands.Users.DeleteUser;
using MyCRM.Auth.Application.DTOs;
using MyCRM.GraphQL.GraphQL.Auth;
using NSubstitute;

namespace MyCRM.UnitTests.GraphQL;

public sealed class AuthMutationTests
{
    private static async Task<IRequestExecutor> BuildExecutorAsync(ISender sender, IMediator mediator)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddSingleton(sender);
        services.AddSingleton(mediator);

        return await services
            .AddGraphQLServer()
            .AddQueryType()
            .AddTypeExtension<TestQuery>()
            .AddMutationType()
            .AddTypeExtension<AuthMutations>()
            .AddAuthorization()
            .BuildRequestExecutorAsync();
    }

    private static IOperationRequest BuildRequest(string query, bool authenticated)
    {
        var builder = new OperationRequestBuilder().SetDocument(query);

        if (authenticated)
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            ], authenticationType: "Bearer"));

            builder.AddGlobalState(nameof(ClaimsPrincipal), principal);
        }

        return builder.Build();
    }

    [Fact]
    public async Task Schema_CreateUser_FieldExistsOnMutationType()
    {
        var sender = Substitute.For<ISender>();
        var mediator = Substitute.For<IMediator>();
        var executor = await BuildExecutorAsync(sender, mediator);

        Assert.True(executor.Schema.MutationType!.Fields.ContainsField("createUser"));
    }

    [Fact]
    public async Task Schema_Login_FieldExistsOnMutationType()
    {
        var sender = Substitute.For<ISender>();
        var mediator = Substitute.For<IMediator>();
        var executor = await BuildExecutorAsync(sender, mediator);

        Assert.True(executor.Schema.MutationType!.Fields.ContainsField("login"));
    }

    [Fact]
    public async Task Schema_RefreshToken_FieldExistsOnMutationType()
    {
        var sender = Substitute.For<ISender>();
        var mediator = Substitute.For<IMediator>();
        var executor = await BuildExecutorAsync(sender, mediator);

        Assert.True(executor.Schema.MutationType!.Fields.ContainsField("refreshToken"));
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsLoginDto()
    {
        var sender = Substitute.For<ISender>();
        var mediator = Substitute.For<IMediator>();

        var dto = new LoginDto(
            Token: "new-access-token",
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(1),
            UserId: Guid.NewGuid(),
            Name: "Test User",
            Email: "test@test.com",
            RefreshToken: "new-refresh-token",
            RefreshTokenExpiresAt: DateTimeOffset.UtcNow.AddDays(30));

        mediator.Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<LoginDto>.Success(dto));

        var executor = await BuildExecutorAsync(sender, mediator);

        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { refreshToken(input: { tenantId: \"00000000-0000-0000-0000-000000000001\", refreshToken: \"some-token\" }) { token refreshToken email } }",
            authenticated: false))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var data = result.Data!["refreshToken"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(data);
        Assert.Equal("new-access-token", data["token"]?.ToString());
        Assert.Equal("new-refresh-token", data["refreshToken"]?.ToString());
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsLoginDtoWithRefreshToken()
    {
        var sender = Substitute.For<ISender>();
        var mediator = Substitute.For<IMediator>();

        var dto = new LoginDto(
            Token: "access-token",
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(1),
            UserId: Guid.NewGuid(),
            Name: "Test User",
            Email: "test@test.com",
            RefreshToken: "refresh-token-value",
            RefreshTokenExpiresAt: DateTimeOffset.UtcNow.AddDays(30));

        mediator.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<LoginDto>.Success(dto));

        var executor = await BuildExecutorAsync(sender, mediator);

        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { login(input: { tenantId: \"00000000-0000-0000-0000-000000000001\", email: \"test@test.com\", password: \"Password123!\" }) { token refreshToken expiresAt refreshTokenExpiresAt } }",
            authenticated: false))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var data = result.Data!["login"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(data);
        Assert.Equal("access-token", data["token"]?.ToString());
        Assert.Equal("refresh-token-value", data["refreshToken"]?.ToString());
    }

    [Fact]
    public async Task Schema_DeleteUser_FieldExistsOnMutationType()
    {
        var sender = Substitute.For<ISender>();
        var mediator = Substitute.For<IMediator>();
        var executor = await BuildExecutorAsync(sender, mediator);

        Assert.True(executor.Schema.MutationType!.Fields.ContainsField("deleteUser"));
    }

    [Fact]
    public async Task DeleteUser_WithToken_ReturnsTrue()
    {
        var sender = Substitute.For<ISender>();
        var mediator = Substitute.For<IMediator>();

        sender.Send(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result.Success());

        var executor = await BuildExecutorAsync(sender, mediator);
        var userId = Guid.NewGuid();

        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteUser(id: \"{userId}\") }}",
            authenticated: true))).ExpectOperationResult();

        Assert.Null(result.Errors);
        Assert.Equal(true, result.Data!["deleteUser"]);
    }

    [Fact]
    public async Task DeleteUser_UserNotFound_ReturnsGraphQLError()
    {
        var sender = Substitute.For<ISender>();
        var mediator = Substitute.For<IMediator>();

        sender.Send(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result.Failure("USER_NOT_FOUND", "User not found."));

        var executor = await BuildExecutorAsync(sender, mediator);
        var userId = Guid.NewGuid();

        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteUser(id: \"{userId}\") }}",
            authenticated: true))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Equal("USER_NOT_FOUND", result.Errors[0].Extensions?["code"]?.ToString());
    }

    [Fact]
    public async Task CreateUser_WithoutToken_ReturnsAuthorizationError()
    {
        var sender = Substitute.For<ISender>();
        var mediator = Substitute.For<IMediator>();
        var executor = await BuildExecutorAsync(sender, mediator);

        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createUser(input: { name: \"Maria\", email: \"maria@test.com\", password: \"Password123!\" }) { id name email } }",
            authenticated: false))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task CreateUser_WithToken_ReturnsCreatedUser()
    {
        var sender = Substitute.For<ISender>();
        var mediator = Substitute.For<IMediator>();

        var dto = new UserDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Name: "Maria",
            Email: "maria@test.com",
            IsActive: true,
            PersonId: null,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null,
            CreatedBy: null,
            UpdatedBy: null);

        sender.Send(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<UserDto>.Success(dto));

        var executor = await BuildExecutorAsync(sender, mediator);

        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createUser(input: { name: \"Maria\", email: \"maria@test.com\", password: \"Password123!\" }) { id name email isActive } }",
            authenticated: true))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var created = result.Data!["createUser"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(created);
        Assert.Equal("Maria", created["name"]?.ToString());
        Assert.Equal("maria@test.com", created["email"]?.ToString());
    }
}

[ExtendObjectType(OperationTypeNames.Query)]
internal sealed class TestQuery
{
    public string Ping() => "pong";
}
