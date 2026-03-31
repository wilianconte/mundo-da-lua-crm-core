using System.Security.Claims;
using HotChocolate.Execution;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Queries.GetMyPermissions;
using MyCRM.Auth.Application.Queries.GetPermissions;
using MyCRM.GraphQL.GraphQL.Auth;
using MyCRM.Shared.Kernel;
using NSubstitute;

namespace MyCRM.UnitTests.GraphQL;

public sealed class PermissionQueryAuthorizationTests
{
    private static async Task<IRequestExecutor> BuildExecutorAsync(ISender sender)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(opts =>
        {
            foreach (var (name, _) in SystemPermissions.All)
                opts.AddPolicy(name, b => b.RequireClaim("permission", name));
        });
        services.AddSingleton(sender);

        return await services
            .AddGraphQLServer()
            .AddQueryType()
            .AddTypeExtension<PermissionQueries>()
            .AddTypeExtension<PermissionAdminQueries>()
            .AddAuthorization()
            .BuildRequestExecutorAsync();
    }

    private static IOperationRequest BuildRequest(string query, bool includeRolesManageClaim = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        };

        if (includeRolesManageClaim)
            claims.Add(new Claim("permission", SystemPermissions.RolesManage));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Bearer"));

        return new OperationRequestBuilder()
            .SetDocument(query)
            .AddGlobalState(nameof(ClaimsPrincipal), principal)
            .Build();
    }

    [Fact]
    public async Task MyPermissions_AuthenticatedWithoutRolesManage_MustSucceed()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetMyPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<IReadOnlyList<string>>.Success(["people:read", "people:update"]));

        var executor = await BuildExecutorAsync(sender);

        var result = (await executor.ExecuteAsync(BuildRequest("{ myPermissions }"))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var permissions = result.Data!["myPermissions"] as IReadOnlyList<object?>;
        Assert.NotNull(permissions);
        Assert.Contains("people:read", permissions.Select(x => x?.ToString()));
    }

    [Fact]
    public async Task Permissions_WithoutRolesManage_MustReturnAuthorizationError()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<IReadOnlyList<PermissionDto>>.Success([]));

        var executor = await BuildExecutorAsync(sender);

        var result = (await executor.ExecuteAsync(BuildRequest("{ permissions { id name } }"))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }
}
