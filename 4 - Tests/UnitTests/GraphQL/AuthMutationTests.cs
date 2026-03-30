using System.Security.Claims;
using HotChocolate.Execution;
using HotChocolate.Types;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.Auth.Application.Commands.Users.CreateUser;
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
