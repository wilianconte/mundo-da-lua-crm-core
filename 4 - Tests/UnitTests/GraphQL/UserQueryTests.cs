using System.Security.Claims;
using HotChocolate.Execution;
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
/// Schema and execution tests for User queries using the real production naming:
/// - users
/// - userById
/// </summary>
public sealed class UserQueryTests
{
    private static readonly Guid TenantId = new("00000000-0000-0000-0000-000000000001");

    private static AuthDbContext CreateInMemoryDbContext()
    {
        var tenantSvc = Substitute.For<ITenantService>();
        tenantSvc.TenantId.Returns(TenantId);

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
        services.AddAuthorization();
        services.AddSingleton(db);

        return await services
            .AddGraphQLServer()
            .AddQueryType()
            .AddTypeExtension<UserQueries>()
            .AddType<UserObjectType>()
            .AddType<RoleObjectType>()
            .AddAuthorization()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .BuildRequestExecutorAsync();
    }

    private static ClaimsPrincipal AuthenticatedUser(Guid userId) =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        ], authenticationType: "Bearer"));

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

    [Fact]
    public async Task Schema_Users_FieldExistsOnQueryType()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        Assert.True(executor.Schema.QueryType.Fields.ContainsField("users"),
            "The field 'users' must exist on Query");
    }

    [Fact]
    public async Task Schema_UserById_FieldExistsOnQueryType()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        Assert.True(executor.Schema.QueryType.Fields.ContainsField("userById"),
            "The field 'userById' must exist on Query");
    }

    [Fact]
    public async Task Schema_GetPrefixedUserFields_DoNotExistOnQueryType()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);
        var queryType = executor.Schema.QueryType;

        Assert.False(queryType.Fields.ContainsField("getUsers"));
        Assert.False(queryType.Fields.ContainsField("getUserById"));
        Assert.False(queryType.Fields.ContainsField("getUser"));
        Assert.False(queryType.Fields.ContainsField("user"));
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
            "createdAt", "updatedAt", "createdBy", "updatedBy", "roles",
        };

        foreach (var field in expectedFields)
            Assert.True(userType.Fields.ContainsField(field),
                $"The field '{field}' must be exposed on User");
    }

    [Fact]
    public async Task Schema_UserType_DoesNotExposeSensitiveFields()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);
        var userType = executor.Schema.GetType<HotChocolate.Types.ObjectType>("User");

        Assert.False(userType.Fields.ContainsField("passwordHash"));
        Assert.False(userType.Fields.ContainsField("tenantId"));
        Assert.False(userType.Fields.ContainsField("isDeleted"));
    }

    [Fact]
    public async Task Users_WithoutToken_ReturnsAuthorizationError()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var result = (await executor.ExecuteAsync(
            "{ users { nodes { id name email isActive } } }"))
            .ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task UserById_WithoutToken_ReturnsAuthorizationError()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var result = (await executor.ExecuteAsync(
            $"{{ userById(id: \"{Guid.NewGuid()}\") {{ id name }} }}"))
            .ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task Users_WithToken_EmptyDatabase_ReturnsEmptyNodes()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ users { totalCount nodes { id name email isActive } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var users = result.Data!["users"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(users);
        Assert.Equal(0, Convert.ToInt32(users["totalCount"]));
    }

    [Fact]
    public async Task Users_WithToken_ReturnsSeededUsers()
    {
        await using var db = CreateInMemoryDbContext();

        var alice = CreateUser("Alice Silva", "alice@test.com");
        var bob = CreateUser("Bob Santos", "bob@test.com");
        await db.Users.AddRangeAsync(alice, bob);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ users(first: 10) { totalCount nodes { id name email isActive } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var users = result.Data!["users"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(2, Convert.ToInt32(users!["totalCount"]));
    }

    [Fact]
    public async Task Users_FilterByIsActive_ReturnsOnlyActiveUsers()
    {
        await using var db = CreateInMemoryDbContext();

        var active = CreateUser("Ativo", "ativo@test.com", isActive: true);
        var inactive = CreateUser("Inativo", "inativo@test.com", isActive: false);
        await db.Users.AddRangeAsync(active, inactive);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ users(where: { isActive: { eq: true } }) { totalCount nodes { id isActive } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var users = result.Data!["users"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(1, Convert.ToInt32(users!["totalCount"]));
    }

    [Fact]
    public async Task Users_FilterByEmailContains_ReturnsMatchingUsers()
    {
        await using var db = CreateInMemoryDbContext();

        var userA = CreateUser("Alice", "alice@mundodalua.com");
        var userB = CreateUser("Carlos", "carlos@gmail.com");
        await db.Users.AddRangeAsync(userA, userB);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ users(where: { email: { contains: \"mundodalua\" } }) { totalCount nodes { email } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var users = result.Data!["users"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(1, Convert.ToInt32(users!["totalCount"]));
    }

    [Fact]
    public async Task Users_Pagination_FirstReturnsLimitedResults()
    {
        await using var db = CreateInMemoryDbContext();

        for (var i = 1; i <= 5; i++)
            await db.Users.AddAsync(CreateUser($"User {i}", $"user{i}@test.com"));
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ users(first: 3) { totalCount pageInfo { hasNextPage } nodes { id } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var users = result.Data!["users"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(5, Convert.ToInt32(users!["totalCount"]));

        var pageInfo = users["pageInfo"] as IReadOnlyDictionary<string, object?>;
        Assert.True((bool)pageInfo!["hasNextPage"]!);

        var nodes = users["nodes"] as IReadOnlyList<object?>;
        Assert.Equal(3, nodes!.Count);
    }

    [Fact]
    public async Task UserById_WithToken_ExistingUser_ReturnsUser()
    {
        await using var db = CreateInMemoryDbContext();

        var user = CreateUser("Alice", "alice@test.com");
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest($"{{ userById(id: \"{user.Id}\") {{ id name email isActive }} }}");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var found = result.Data!["userById"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(found);
        Assert.Equal(user.Id.ToString(), found["id"]?.ToString());
        Assert.Equal("Alice", found["name"]?.ToString());
    }

    [Fact]
    public async Task UserById_WithToken_ReturnsRoles()
    {
        await using var db = CreateInMemoryDbContext();

        var user = CreateUser("Alice", "alice@test.com");
        var role = Role.Create(TenantId, "Administrador", "Acesso total");
        await db.Users.AddAsync(user);
        await db.Roles.AddAsync(role);
        await db.UserRoles.AddAsync(UserRole.Create(user.Id, role.Id));
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest(
            $"{{ userById(id: \"{user.Id}\") {{ id roles {{ id name description isActive }} }} }}");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var found = result.Data!["userById"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(found);

        var roles = found["roles"] as IReadOnlyList<object?>;
        Assert.NotNull(roles);
        Assert.Single(roles);
    }

    [Fact]
    public async Task Users_WithToken_ReturnsRoles()
    {
        await using var db = CreateInMemoryDbContext();

        var user = CreateUser("Bob", "bob@test.com");
        var role = Role.Create(TenantId, "Professor", "Docencia");
        await db.Users.AddAsync(user);
        await db.Roles.AddAsync(role);
        await db.UserRoles.AddAsync(UserRole.Create(user.Id, role.Id));
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);
        var request = BuildRequest("{ users(first: 10) { nodes { id roles { id name description isActive } } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var users = result.Data!["users"] as IReadOnlyDictionary<string, object?>;
        var nodes = users!["nodes"] as IReadOnlyList<object?>;
        var firstNode = nodes![0] as IReadOnlyDictionary<string, object?>;
        var roles = firstNode!["roles"] as IReadOnlyList<object?>;
        Assert.NotNull(roles);
        Assert.Single(roles);
    }

    [Fact]
    public async Task UserById_WithToken_NonExistingUser_ReturnsNull()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest($"{{ userById(id: \"{Guid.NewGuid()}\") {{ id name }} }}");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        Assert.Null(result.Data!["userById"]);
    }

    [Fact]
    public async Task Users_SoftDeleted_AreNotReturned()
    {
        await using var db = CreateInMemoryDbContext();

        var active = CreateUser("Ativo", "ativo@test.com");
        var deleted = CreateUser("Deletado", "del@test.com");
        deleted.SoftDelete();

        await db.Users.AddRangeAsync(active, deleted);
        await db.SaveChangesAsync();

        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ users { totalCount nodes { id name } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.Null(result.Errors);
        var users = result.Data!["users"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal(1, Convert.ToInt32(users!["totalCount"]));
    }

    [Fact]
    public async Task GetUsers_WithToken_ReturnsFieldNotFoundError()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest("{ getUsers(first: 10) { totalCount nodes { id } } }");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task GetUserById_WithToken_ReturnsFieldNotFoundError()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest($"{{ getUserById(id: \"{Guid.NewGuid()}\") {{ id name }} }}");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task GetUser_WithToken_ReturnsFieldNotFoundError()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest($"{{ getUser(id: \"{Guid.NewGuid()}\") {{ id name }} }}");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task User_WithToken_ReturnsFieldNotFoundError()
    {
        await using var db = CreateInMemoryDbContext();
        var executor = await BuildExecutorAsync(db);

        var request = BuildRequest($"{{ user(id: \"{Guid.NewGuid()}\") {{ id name }} }}");
        var result = (await executor.ExecuteAsync(request)).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }
}
