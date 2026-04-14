using System.Security.Claims;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Infrastructure.Persistence;
using MyCRM.GraphQL.GraphQL.Tenants;
using MyCRM.Shared.Kernel;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.GraphQL;

/// <summary>
/// Schema and execution tests for Tenant queries:
/// - tenants (paginado, filtrável, ordenável)
/// - tenantById
/// </summary>
public sealed class TenantQueryTests
{
    private static AuthDbContext CreateInMemoryDbContext()
    {
        var tenantSvc = Substitute.For<ITenantService>();
        tenantSvc.TenantId.Returns(Guid.NewGuid());

        var currentUserSvc = Substitute.For<ICurrentUserService>();
        currentUserSvc.UserId.Returns((Guid?)null);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, tenantSvc, currentUserSvc);
    }

    private static async Task<IRequestExecutor> BuildExecutorAsync(AuthDbContext db)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(opts =>
        {
            foreach (var (name, _) in SystemPermissions.All)
                opts.AddPolicy(name, b => b.RequireAuthenticatedUser());
        });
        services.AddSingleton(db);

        return await services
            .AddGraphQLServer()
            .AddQueryType()
            .AddTypeExtension<TenantQueries>()
            .AddAuthorization()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .BuildRequestExecutorAsync();
    }

    private static IOperationRequest BuildRequest(string query, Guid? userId = null)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, (userId ?? Guid.NewGuid()).ToString()),
        ], authenticationType: "Bearer"));

        return new OperationRequestBuilder()
            .SetDocument(query)
            .AddGlobalState(nameof(ClaimsPrincipal), principal)
            .Build();
    }

    private static Tenant CreateTenant(string name, TenantStatus status = TenantStatus.Active)
    {
        var tenant = Tenant.Create(name, Guid.NewGuid());
        switch (status)
        {
            case TenantStatus.Active:    tenant.Activate();  break;
            case TenantStatus.Suspended: tenant.Suspend();   break;
            case TenantStatus.Cancelled: tenant.Cancel();    break;
        }
        return tenant;
    }

    // ─── Schema ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Schema_Tenants_FieldExistsOnQueryType()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        Assert.True(executor.Schema.QueryType.Fields.ContainsField("tenants"),
            "The field 'tenants' must exist on Query");
    }

    [Fact]
    public async Task Schema_TenantById_FieldExistsOnQueryType()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        Assert.True(executor.Schema.QueryType.Fields.ContainsField("tenantById"),
            "The field 'tenantById' must exist on Query");
    }

    // ─── Authorization ────────────────────────────────────────────────────────

    [Fact]
    public async Task Tenants_WithoutToken_ReturnsAuthorizationError()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var result = (await executor.ExecuteAsync(
            "{ tenants { nodes { id name } } }"))
            .ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task TenantById_WithoutToken_ReturnsAuthorizationError()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var result = (await executor.ExecuteAsync(
            $"{{ tenantById(id: \"{Guid.NewGuid()}\") {{ id name }} }}"))
            .ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    // ─── Queries ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Tenants_WithToken_EmptyDatabase_ReturnsEmptyNodes()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ tenants { totalCount nodes { id name } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var tenants = result.Data!["tenants"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(0, Convert.ToInt32(tenants!["totalCount"]));
    }

    [Fact]
    public async Task Tenants_WithToken_ReturnsSeededTenants()
    {
        await using var db = CreateInMemoryDbContext();
        await db.Tenants.AddRangeAsync(CreateTenant("Tenant A"), CreateTenant("Tenant B"));
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);
        var request = BuildRequest("{ tenants(first: 10) { totalCount nodes { id name } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var tenants = result.Data!["tenants"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(2, Convert.ToInt32(tenants!["totalCount"]));
    }

    [Fact]
    public async Task TenantById_WithToken_ExistingTenant_ReturnsTenant()
    {
        await using var db = CreateInMemoryDbContext();
        var tenant = CreateTenant("Acme Ltda");
        await db.Tenants.AddAsync(tenant);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);
        var request = BuildRequest($"{{ tenantById(id: \"{tenant.Id}\") {{ id name }} }}");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var found = result.Data!["tenantById"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(found);
        Assert.Equal(tenant.Id.ToString(), found["id"]?.ToString());
        Assert.Equal("Acme Ltda", found["name"]?.ToString());
    }

    [Fact]
    public async Task TenantById_WithToken_NonExistingTenant_ReturnsNull()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest($"{{ tenantById(id: \"{Guid.NewGuid()}\") {{ id name }} }}");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        Assert.Null(result.Data!["tenantById"]);
    }

    [Fact]
    public async Task Tenants_SoftDeleted_AreNotReturned()
    {
        await using var db = CreateInMemoryDbContext();
        var active = CreateTenant("Ativo");
        var deleted = CreateTenant("Deletado");
        deleted.SoftDelete();

        await db.Tenants.AddRangeAsync(active, deleted);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);
        var request = BuildRequest("{ tenants { totalCount nodes { id name } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var tenants = result.Data!["tenants"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(1, Convert.ToInt32(tenants!["totalCount"]));
    }

    [Fact]
    public async Task Tenants_Pagination_FirstReturnsLimitedResults()
    {
        await using var db = CreateInMemoryDbContext();
        for (var i = 1; i <= 5; i++)
            await db.Tenants.AddAsync(CreateTenant($"Tenant {i}"));
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);
        var request = BuildRequest("{ tenants(first: 3) { totalCount pageInfo { hasNextPage } nodes { id } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var tenants = result.Data!["tenants"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(5, Convert.ToInt32(tenants!["totalCount"]));

        var pageInfo = tenants["pageInfo"] as IReadOnlyDictionary<string, object?>;
        Assert.True((bool)pageInfo!["hasNextPage"]!);

        var nodes = tenants["nodes"] as IReadOnlyList<object?>;
        Assert.Equal(3, nodes!.Count);
    }

    [Fact]
    public async Task Tenants_ReturnsTenantFromAllTenantContexts()
    {
        // Diferente dos outros endpoints, Tenants não filtra por TenantId —
        // é uma visão global da plataforma (apenas soft-delete).
        await using var db = CreateInMemoryDbContext();
        await db.Tenants.AddRangeAsync(
            Tenant.Create("Tenant X", Guid.NewGuid()),
            Tenant.Create("Tenant Y", Guid.NewGuid()));
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);
        var request = BuildRequest("{ tenants { totalCount nodes { id } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var tenants = result.Data!["tenants"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(2, Convert.ToInt32(tenants!["totalCount"]));
    }
}
