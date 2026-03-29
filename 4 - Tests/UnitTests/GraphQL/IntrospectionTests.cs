using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace MyCRM.UnitTests.GraphQL;

public sealed class IntrospectionTests
{
    private static async Task<IRequestExecutor> BuildExecutorAsync(bool disableIntrospection)
    {
        var services = new ServiceCollection()
            .AddGraphQLServer()
            .AddQueryType(d => d.Name("Query").Field("ping").Resolve("pong"))
            .DisableIntrospection(disableIntrospection)
            .Services
            .BuildServiceProvider();

        return await services
            .GetRequiredService<IRequestExecutorResolver>()
            .GetRequestExecutorAsync();
    }

    [Fact]
    public async Task Introspection_InDevelopment_SchemaQueryReturnsData()
    {
        var executor = await BuildExecutorAsync(disableIntrospection: false);

        var result = (await executor.ExecuteAsync("{ __schema { types { name } } }"))
            .ExpectOperationResult();

        Assert.Null(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task Introspection_InProduction_SchemaQueryReturnsError()
    {
        var executor = await BuildExecutorAsync(disableIntrospection: true);

        var result = (await executor.ExecuteAsync("{ __schema { types { name } } }"))
            .ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task Introspection_InProduction_TypeQueryReturnsError()
    {
        var executor = await BuildExecutorAsync(disableIntrospection: true);

        var result = (await executor.ExecuteAsync("{ __type(name: \"Query\") { name } }"))
            .ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task Introspection_InProduction_NormalQueriesStillWork()
    {
        var executor = await BuildExecutorAsync(disableIntrospection: true);

        var result = (await executor.ExecuteAsync("{ ping }"))
            .ExpectOperationResult();

        Assert.Null(result.Errors);
        Assert.Equal("pong", result.Data!["ping"]?.ToString());
    }
}
