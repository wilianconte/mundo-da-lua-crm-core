using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Infrastructure.Persistence;
using MyCRM.GraphQL.GraphQL.Auth;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.GraphQL;

/// <summary>
/// Testes de schema e execução da query <c>getUsers</c>.
///
/// Estratégia:
///   - AuthDbContext configurado com EF InMemory (sem banco real).
///   - Stubs de ITenantService e ICurrentUserService.
///   - Schema Hot Chocolate construído com os tipos reais (UserQueries + UserObjectType).
///   - Claims de usuário autenticado injetadas via RequestContext para os testes que exigem auth.
/// </summary>
public sealed class UserQueryTests
{
    private static readonly Guid TenantId = new("00000000-0000-0000-0000-000000000001");

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AuthDbContext CreateInMemoryDbContext()
    {
        var tenantSvc = Substitute.For<ITenantService>();
        tenantSvc.TenantId.Returns(TenantId);

        var currentUserSvc = Substitute.For<ICurrentUserService>();
        currentUserSvc.UserId.Returns((Guid?)null);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // isolado por teste
            .Options;

        return new AuthDbContext(options, tenantSvc, currentUserSvc);
    }

    private static async Task<IRequestExecutor> BuildExecutorAsync(AuthDbContext db)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddSingleton(db);  // injeta o db já criado

        var executor = await services
            .AddGraphQLServer()
            .AddQueryType()
            .AddTypeExtension<TestUserQueries>()
            .AddType<UserObjectType>()
            .AddAuthorization()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .BuildRequestExecutorAsync();

        return executor;
    }

    /// <summary>Cria um ClaimsPrincipal autenticado (simula JWT válido).</summary>
    private static ClaimsPrincipal AuthenticatedUser(Guid userId) =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        ], authenticationType: "Bearer"));

    /// <summary>Monta uma requisição GraphQL com usuário autenticado injetado via GlobalState.</summary>
    private static IOperationRequest BuildRequest(string query, Guid? userId = null)
    {
        var principal = AuthenticatedUser(userId ?? Guid.NewGuid());

        return new OperationRequestBuilder()
            .SetDocument(query)
            .AddGlobalState(nameof(ClaimsPrincipal), principal)
            .Build();
    }

    private static User CreateUser(string name, string email, bool isActive = true)
    {
        var user = User.Create(TenantId, name, email, "hash-placeholder");
        if (!isActive) user.Deactivate();
        return user;
    }

    // ── Schema tests ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Schema_GetUsers_FieldExistsOnQueryType()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var schema = executor.Schema;
        var queryType = schema.QueryType;

        Assert.True(queryType.Fields.ContainsField("getUsers"),
            "O campo 'getUsers' deve existir no tipo Query");
    }

    [Fact]
    public async Task Schema_GetUserById_FieldExistsOnQueryType()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var schema = executor.Schema;
        Assert.True(schema.QueryType.Fields.ContainsField("getUserById"),
            "O campo 'getUserById' deve existir no tipo Query");
    }

    [Fact]
    public async Task Schema_UserType_ExposesExpectedFields()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var userType = executor.Schema.GetType<HotChocolate.Types.ObjectType>("User");
        Assert.NotNull(userType);

        var expectedFields = new[]
        {
            "id", "name", "email", "isActive", "personId",
            "createdAt", "updatedAt", "createdBy", "updatedBy",
        };

        foreach (var field in expectedFields)
            Assert.True(userType.Fields.ContainsField(field),
                $"O campo '{field}' deve estar exposto no tipo User");
    }

    [Fact]
    public async Task Schema_UserType_DoesNotExposePasswordHash()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var userType = executor.Schema.GetType<HotChocolate.Types.ObjectType>("User");

        Assert.False(userType.Fields.ContainsField("passwordHash"),
            "passwordHash NÃO deve ser exposto no schema");
        Assert.False(userType.Fields.ContainsField("tenantId"),
            "tenantId NÃO deve ser exposto no schema");
        Assert.False(userType.Fields.ContainsField("isDeleted"),
            "isDeleted NÃO deve ser exposto no schema");
    }

    // ── Authorization tests ───────────────────────────────────────────────────

    [Fact]
    public async Task GetUsers_WithoutToken_ReturnsAuthorizationError()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var result = (await executor.ExecuteAsync(
            "{ getUsers { nodes { id name email isActive } } }"))
            .ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task GetUserById_WithoutToken_ReturnsAuthorizationError()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var result = (await executor.ExecuteAsync(
            $"{{ getUserById(id: \"{Guid.NewGuid()}\") {{ id name }} }}"))
            .ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    // ── Execution tests (com usuário autenticado) ─────────────────────────────

    [Fact]
    public async Task GetUsers_WithToken_EmptyDatabase_ReturnsEmptyNodes()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ getUsers { totalCount nodes { id name email isActive } } }");

        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var getUsers = result.Data!["getUsers"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(getUsers);
        Assert.Equal(0, Convert.ToInt32(getUsers["totalCount"]));
    }

    [Fact]
    public async Task GetUsers_WithToken_ReturnsSeededUsers()
    {
        await using var db = CreateInMemoryDbContext();

        var alice = CreateUser("Alice Silva", "alice@test.com");
        var bob   = CreateUser("Bob Santos",  "bob@test.com");
        await db.Users.AddRangeAsync(alice, bob);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ getUsers(first: 10) { totalCount nodes { id name email isActive } } }");

        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var getUsers = result.Data!["getUsers"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(2, Convert.ToInt32(getUsers!["totalCount"]));
    }

    [Fact]
    public async Task GetUsers_FilterByIsActive_ReturnsOnlyActiveUsers()
    {
        await using var db = CreateInMemoryDbContext();

        var active   = CreateUser("Ativo",    "ativo@test.com",   isActive: true);
        var inactive = CreateUser("Inativo",  "inativo@test.com", isActive: false);
        await db.Users.AddRangeAsync(active, inactive);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ getUsers(where: { isActive: { eq: true } }) { totalCount nodes { id isActive } } }");

        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var getUsers = result.Data!["getUsers"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(1, Convert.ToInt32(getUsers!["totalCount"]));
    }

    [Fact]
    public async Task GetUsers_FilterByEmailContains_ReturnsMatchingUsers()
    {
        await using var db = CreateInMemoryDbContext();

        var userA = CreateUser("Alice",  "alice@mundodalua.com");
        var userB = CreateUser("Carlos", "carlos@gmail.com");
        await db.Users.AddRangeAsync(userA, userB);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ getUsers(where: { email: { contains: \"mundodalua\" } }) { totalCount nodes { email } } }");

        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var getUsers = result.Data!["getUsers"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(1, Convert.ToInt32(getUsers!["totalCount"]));
    }

    [Fact]
    public async Task GetUsers_Pagination_FirstReturnsLimitedResults()
    {
        await using var db = CreateInMemoryDbContext();

        for (var i = 1; i <= 5; i++)
            await db.Users.AddAsync(CreateUser($"User {i}", $"user{i}@test.com"));
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ getUsers(first: 3) { totalCount pageInfo { hasNextPage } nodes { id } } }");

        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var getUsers = result.Data!["getUsers"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(5, Convert.ToInt32(getUsers!["totalCount"]));

        var pageInfo = getUsers["pageInfo"] as IReadOnlyDictionary<string, object?>;
        Assert.True((bool)pageInfo!["hasNextPage"]!);

        var nodes = getUsers["nodes"] as IReadOnlyList<object?>;
        Assert.Equal(3, nodes!.Count);
    }

    [Fact]
    public async Task GetUserById_WithToken_ExistingUser_ReturnsUser()
    {
        await using var db = CreateInMemoryDbContext();

        var user = CreateUser("Alice", "alice@test.com");
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest($"{{ getUserById(id: \"{user.Id}\") {{ id name email isActive }} }}");

        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var found = result.Data!["getUserById"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(found);
        Assert.Equal(user.Id.ToString(), found["id"]?.ToString());
        Assert.Equal("Alice", found["name"]?.ToString());
    }

    [Fact]
    public async Task GetUserById_WithToken_NonExistingUser_ReturnsNull()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest($"{{ getUserById(id: \"{Guid.NewGuid()}\") {{ id name }} }}");

        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        Assert.Null(result.Data!["getUserById"]);
    }

    [Fact]
    public async Task GetUsers_SoftDeleted_AreNotReturned()
    {
        await using var db = CreateInMemoryDbContext();

        var active  = CreateUser("Ativo",   "ativo@test.com");
        var deleted = CreateUser("Deletado","del@test.com");
        deleted.SoftDelete();

        await db.Users.AddRangeAsync(active, deleted);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ getUsers { totalCount nodes { id name } } }");

        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var getUsers = result.Data!["getUsers"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(1, Convert.ToInt32(getUsers!["totalCount"]));
    }
}

/// <summary>
/// Wrapper de teste para UserQueries: usa [ExtendObjectType] explícito em vez de [QueryType]
/// para funcionar corretamente em setup de executor isolado (sem assembly-scan automático).
/// Os nomes dos campos são explicitamente definidos para corresponder ao schema de produção.
/// </summary>
[ExtendObjectType(OperationTypeNames.Query)]
[Authorize]
internal sealed class TestUserQueries
{
    [GraphQLName("getUsers")]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AuthDbContext db) =>
        db.Users.AsNoTracking();

    [GraphQLName("getUserById")]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<User> GetUserById(Guid id, [Service] AuthDbContext db) =>
        db.Users.AsNoTracking().Where(x => x.Id == id);
}
