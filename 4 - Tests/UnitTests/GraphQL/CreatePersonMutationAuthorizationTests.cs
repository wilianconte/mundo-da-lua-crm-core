using System.Security.Claims;
using HotChocolate.Execution;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.Auth.Application.Services;
using MyCRM.CRM.Application.Commands.People.CreatePerson;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using MyCRM.GraphQL.Authorization;
using MyCRM.GraphQL.GraphQL.Auth;
using MyCRM.GraphQL.GraphQL.People;
using MyCRM.Shared.Kernel;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.GraphQL;

/// <summary>
/// Regression tests ensuring that class-level [Authorize] on RoleMutations
/// does NOT bleed into PersonMutations (or any other mutation type extension).
/// Root cause: RoleMutations previously had [Authorize(Policy = RolesManage)] at
/// class level, which Hot Chocolate applied to all merged Mutation type fields,
/// denying non-admin users even when they held the correct field-level permission.
/// </summary>
public sealed class CreatePersonMutationAuthorizationTests
{
    private sealed class FixedTenantService(Guid tenantId) : ITenantService
    {
        public Guid TenantId { get; private set; } = tenantId;
        public void SetTenant(Guid id) => TenantId = id;
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
        services.AddDbContext<CRMDbContext>(o => o.UseInMemoryDatabase($"crm-create-person-{Guid.NewGuid()}"));

        return await services
            .AddGraphQLServer()
            .AddQueryType()
            .AddMutationType()
            .AddTypeExtension<PersonQueries>()
            .AddTypeExtension<PersonMutations>()
            .AddTypeExtension<RoleMutations>()
            .AddAuthorization()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .BuildRequestExecutorAsync();
    }

    private static IOperationRequest BuildRequest(string document, Guid userId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("sub", userId.ToString()),
        ], authenticationType: "Bearer"));

        return new OperationRequestBuilder()
            .SetDocument(document)
            .AddGlobalState(nameof(ClaimsPrincipal), principal)
            .Build();
    }

    [Fact]
    public async Task NonAdminUser_WithPeopleCreate_MustSucceedOnCreatePerson()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var expectedPerson = new PersonDto(
            Id: Guid.NewGuid(),
            TenantId: tenantId,
            FullName: "João da Silva",
            PreferredName: null,
            DocumentNumber: null,
            BirthDate: null,
            Gender: null,
            MaritalStatus: null,
            Nationality: null,
            Occupation: null,
            Email: null,
            PrimaryPhone: null,
            SecondaryPhone: null,
            WhatsAppNumber: null,
            ProfileImageUrl: null,
            Status: PersonStatus.Active,
            Notes: null,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null);

        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<CreatePersonCommand>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<PersonDto>.Success(expectedPerson));

        var permissionService = Substitute.For<IPermissionService>();
        // Non-admin has people:create but NOT roles:manage
        permissionService.HasPermissionAsync(userId, SystemPermissions.PeopleCreate, Arg.Any<CancellationToken>())
            .Returns(true);
        permissionService.HasPermissionAsync(userId, SystemPermissions.RolesManage, Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(sender, permissionService, tenantId);

        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createPerson(input: { fullName: \"João da Silva\" }) { id fullName } }",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var data = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createPerson"]);
        Assert.Equal("João da Silva", data["fullName"]?.ToString());
    }

    [Fact]
    public async Task NonAdminUser_WithoutPeopleCreate_MustReceiveAuthorizationError()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var sender = Substitute.For<ISender>();

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(sender, permissionService, tenantId);

        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createPerson(input: { fullName: \"Unauthorized\" }) { id fullName } }",
            userId))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors!, e => e.Code == "AUTH_NOT_AUTHORIZED");
    }

    [Fact]
    public async Task NonAdminUser_WithPeopleCreate_ButWithoutRolesManage_MustNotAccessCreateRole()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var sender = Substitute.For<ISender>();

        var permissionService = Substitute.For<IPermissionService>();
        // Has people:create but NOT roles:manage
        permissionService.HasPermissionAsync(userId, SystemPermissions.PeopleCreate, Arg.Any<CancellationToken>())
            .Returns(true);
        permissionService.HasPermissionAsync(userId, SystemPermissions.RolesManage, Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(sender, permissionService, tenantId);

        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createRole(input: { name: \"Hacker\" }) { id name } }",
            userId))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors!, e => e.Code == "AUTH_NOT_AUTHORIZED");
    }
}
