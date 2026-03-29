using HotChocolate.Authorization;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace MyCRM.UnitTests.GraphQL;

public sealed class AuthorizationHandlerRegistrationTests
{
    private static ServiceCollection CreateBaseServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        return services;
    }

    [Fact]
    public void AddAuthorization_RegistersIAuthorizationHandler_InRootProvider()
    {
        var services = CreateBaseServices();
        services.AddGraphQLServer()
            .AddQueryType(d => d.Name("Query").Field("ping").Resolve("pong"))
            .AddAuthorization();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetService<IAuthorizationHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddAuthorization_HandlerIsDefaultAuthorizationHandler()
    {
        var services = CreateBaseServices();
        services.AddGraphQLServer()
            .AddQueryType(d => d.Name("Query").Field("ping").Resolve("pong"))
            .AddAuthorization();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IAuthorizationHandler>();
        Assert.Equal(
            "HotChocolate.AspNetCore.Authorization.DefaultAuthorizationHandler",
            handler.GetType().FullName);
    }

    [Fact]
    public async Task AuthorizedQuery_WithoutToken_ReturnsAuthError()
    {
        var services = CreateBaseServices();
        services.AddGraphQLServer()
            .AddQueryType(d => d.Name("Query")
                .Field("secret")
                .Authorize()
                .Resolve("classified"))
            .AddAuthorization();

        var provider = services.BuildServiceProvider();
        var executor = await provider
            .GetRequiredService<IRequestExecutorResolver>()
            .GetRequestExecutorAsync();

        var result = (await executor.ExecuteAsync("{ secret }"))
            .ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task NonAuthorizedQuery_WithAuthorization_ReturnsData()
    {
        var services = CreateBaseServices();
        services.AddGraphQLServer()
            .AddQueryType(d => d.Name("Query")
                .Field("ping")
                .Resolve("pong"))
            .AddAuthorization();

        var provider = services.BuildServiceProvider();
        var executor = await provider
            .GetRequiredService<IRequestExecutorResolver>()
            .GetRequestExecutorAsync();

        var result = (await executor.ExecuteAsync("{ ping }"))
            .ExpectOperationResult();

        Assert.Null(result.Errors);
        Assert.Equal("pong", result.Data!["ping"]?.ToString());
    }

    [Fact]
    public async Task AuthorizedQuery_ExecutorDoesNotThrowOnEnrich()
    {
        var services = CreateBaseServices();
        services.AddGraphQLServer()
            .AddQueryType(d => d.Name("Query")
                .Field("secret")
                .Authorize()
                .Resolve("classified"))
            .AddAuthorization();

        var provider = services.BuildServiceProvider();
        var executor = await provider
            .GetRequiredService<IRequestExecutorResolver>()
            .GetRequestExecutorAsync();

        var exception = await Record.ExceptionAsync(async () =>
            await executor.ExecuteAsync("{ secret }"));

        Assert.Null(exception);
    }

    /// <summary>
    /// Garante que um campo com [AllowAnonymous] dentro de um tipo que tem [Authorize]
    /// em nível de classe ainda é acessível sem token — cenário do mutation login.
    /// </summary>
    [Fact]
    public async Task AllowAnonymousField_InsideAuthorizedType_IsAccessibleWithoutToken()
    {
        var services = CreateBaseServices();
        services.AddGraphQLServer()
            .AddMutationType(d => d.Name("Mutation"))
            .AddTypeExtension<AuthorizedMutations>()
            .AddTypeExtension<PublicMutations>()
            .AddQueryType(d => d.Name("Query").Field("_").Resolve("ok"))
            .AddAuthorization();

        var provider = services.BuildServiceProvider();
        var executor = await provider
            .GetRequiredService<IRequestExecutorResolver>()
            .GetRequestExecutorAsync();

        var result = (await executor.ExecuteAsync("mutation { login }"))
            .ExpectOperationResult();

        Assert.Null(result.Errors);
        Assert.Equal("token", result.Data!["login"]?.ToString());
    }

    [Authorize]
    [ExtendObjectType(OperationTypeNames.Mutation)]
    private sealed class AuthorizedMutations
    {
        public string SecureOp() => "done";
    }

    [ExtendObjectType(OperationTypeNames.Mutation)]
    private sealed class PublicMutations
    {
        [AllowAnonymous]
        public string Login() => "token";
    }
}
