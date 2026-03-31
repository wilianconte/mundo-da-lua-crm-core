using System.Security.Claims;
using HotChocolate.Execution;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.Auth.Application.Queries.GetMyPermissions;
using MyCRM.Auth.Application.Services;
using MyCRM.CRM.Infrastructure.Persistence;
using MyCRM.GraphQL.Authorization;
using MyCRM.GraphQL.GraphQL.Auth;
using MyCRM.GraphQL.GraphQL.People;
using MyCRM.Shared.Kernel;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.GraphQL;

public sealed class PermissionAndPeopleAuthorizationRegressionTests
{
    private sealed class FixedTenantService(Guid tenantId) : ITenantService
    {
        public Guid TenantId { get; private set; } = tenantId;
        public void SetTenant(Guid tenantIdValue) => TenantId = tenantIdValue;
    }

    private static async Task<IRequestExecutor> BuildExecutorAsync(
        ISender sender,
        IPermissionService permissionService,
        Guid tenantId)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(opts =>
        {
            foreach (var (name, _) in SystemPermissions.All)
                opts.AddPolicy(name, b => b.Requirements.Add(new PermissionRequirement(name)));
        });

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());

        services.AddSingleton(sender);
        services.AddSingleton(permissionService);
        services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<ITenantService>(_ => new FixedTenantService(tenantId));
        services.AddScoped<ICurrentUserService>(_ => currentUserService);
        services.AddDbContext<CRMDbContext>(o => o.UseInMemoryDatabase($"crm-auth-{Guid.NewGuid()}"));

        return await services
            .AddGraphQLServer()
            .AddQueryType()
            .AddTypeExtension<PermissionQueries>()
            .AddTypeExtension<PermissionAdminQueries>()
            .AddTypeExtension<PersonQueries>()
            .AddAuthorization()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .BuildRequestExecutorAsync();
    }

    private static IOperationRequest BuildRequest(string query, Guid userId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("sub", userId.ToString()),
        ], authenticationType: "Bearer"));

        return new OperationRequestBuilder()
            .SetDocument(query)
            .AddGlobalState(nameof(ClaimsPrincipal), principal)
            .Build();
    }

    [Fact]
    public async Task AuthenticatedNonAdmin_WithPeopleRead_MustAccessMyPermissionsAndPeople()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetMyPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<IReadOnlyList<string>>.Success([SystemPermissions.PeopleRead]));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.PeopleRead, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(sender, permissionService, tenantId);

        var result = (await executor.ExecuteAsync(BuildRequest(
            "{ myPermissions people(first: 1) { totalCount } }",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);

        var permissions = Assert.IsAssignableFrom<IReadOnlyList<object?>>(result.Data!["myPermissions"]);
        Assert.Contains(SystemPermissions.PeopleRead, permissions.Select(x => x?.ToString()));

        var people = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["people"]);
        Assert.Equal("0", people["totalCount"]?.ToString());
    }

    [Fact]
    public async Task AuthenticatedNonAdmin_WithoutPeopleRead_MustReceiveAuthorizationErrorOnPeople()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetMyPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<IReadOnlyList<string>>.Success([]));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.PeopleRead, Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(sender, permissionService, tenantId);

        var result = (await executor.ExecuteAsync(BuildRequest(
            "{ people(first: 1) { totalCount } }",
            userId))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors!, e => e.Code == "AUTH_NOT_AUTHORIZED");
    }
}
