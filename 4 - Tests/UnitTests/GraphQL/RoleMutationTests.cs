using System.Security.Claims;
using HotChocolate.Execution;
using HotChocolate.Types;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.Auth.Application.Commands.Roles.CreateRole;
using MyCRM.Auth.Application.Commands.Roles.DeleteRole;
using MyCRM.Auth.Application.Commands.Roles.UpdateRole;
using MyCRM.Auth.Application.DTOs;
using MyCRM.GraphQL.GraphQL.Auth;
using NSubstitute;
using KernelResult = MyCRM.Shared.Kernel.Results.Result;

namespace MyCRM.UnitTests.GraphQL;

public sealed class RoleMutationTests
{
    private static async Task<IRequestExecutor> BuildExecutorAsync(ISender sender)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddSingleton(sender);

        return await services
            .AddGraphQLServer()
            .AddQueryType()
            .AddTypeExtension<RoleTestQuery>()
            .AddMutationType()
            .AddTypeExtension<RoleMutations>()
            .AddAuthorization()
            .BuildRequestExecutorAsync();
    }

    private static IOperationRequest BuildRequest(string query, bool authenticated = true)
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

    private static RoleDto MakeDto(string name = "Admin") => new(
        Id: Guid.NewGuid(),
        TenantId: Guid.NewGuid(),
        Name: name,
        Description: "Acesso total",
        IsActive: true,
        CreatedAt: DateTimeOffset.UtcNow,
        UpdatedAt: null,
        CreatedBy: null,
        UpdatedBy: null);

    // ── Schema presence ──────────────────────────────────────────────────────

    [Fact]
    public async Task Schema_CreateRole_FieldExists()
    {
        var executor = await BuildExecutorAsync(Substitute.For<ISender>());
        Assert.True(executor.Schema.MutationType!.Fields.ContainsField("createRole"));
    }

    [Fact]
    public async Task Schema_UpdateRole_FieldExists()
    {
        var executor = await BuildExecutorAsync(Substitute.For<ISender>());
        Assert.True(executor.Schema.MutationType!.Fields.ContainsField("updateRole"));
    }

    [Fact]
    public async Task Schema_DeleteRole_FieldExists()
    {
        var executor = await BuildExecutorAsync(Substitute.For<ISender>());
        Assert.True(executor.Schema.MutationType!.Fields.ContainsField("deleteRole"));
    }

    // ── CreateRole ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRole_WithToken_ReturnsCreatedRole()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<CreateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<RoleDto>.Success(MakeDto("Admin")));

        var executor = await BuildExecutorAsync(sender);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createRole(input: { name: \"Admin\" }) { name isActive } }"))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var data = result.Data!["createRole"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal("Admin", data!["name"]?.ToString());
        Assert.Equal(true, data["isActive"]);
    }

    [Fact]
    public async Task CreateRole_WithoutToken_ReturnsAuthorizationError()
    {
        var executor = await BuildExecutorAsync(Substitute.For<ISender>());
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createRole(input: { name: \"Admin\" }) { name } }",
            authenticated: false))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task CreateRole_DuplicateName_ReturnsGraphQLError()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<CreateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<RoleDto>.Failure("ROLE_NAME_DUPLICATE", "A role with this name already exists."));

        var executor = await BuildExecutorAsync(sender);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createRole(input: { name: \"Admin\" }) { name } }"))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.Equal("ROLE_NAME_DUPLICATE", result.Errors![0].Extensions?["code"]?.ToString());
    }

    // ── UpdateRole ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateRole_WithToken_ReturnsUpdatedRole()
    {
        var roleId = Guid.NewGuid();
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<RoleDto>.Success(MakeDto("Coordenador")));

        var executor = await BuildExecutorAsync(sender);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateRole(id: \"{roleId}\", input: {{ name: \"Coordenador\" }}) {{ name }} }}"))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var data = result.Data!["updateRole"] as IReadOnlyDictionary<string, object?>;
        Assert.Equal("Coordenador", data!["name"]?.ToString());
    }

    [Fact]
    public async Task UpdateRole_NotFound_ReturnsGraphQLError()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(MyCRM.Shared.Kernel.Results.Result<RoleDto>.Failure("ROLE_NOT_FOUND", "Role not found."));

        var executor = await BuildExecutorAsync(sender);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateRole(id: \"{Guid.NewGuid()}\", input: {{ name: \"X\" }}) {{ name }} }}"))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.Equal("ROLE_NOT_FOUND", result.Errors![0].Extensions?["code"]?.ToString());
    }

    // ── DeleteRole ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteRole_WithToken_ReturnsTrue()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(KernelResult.Success());

        var executor = await BuildExecutorAsync(sender);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteRole(id: \"{Guid.NewGuid()}\") }}"))).ExpectOperationResult();

        Assert.Null(result.Errors);
        Assert.Equal(true, result.Data!["deleteRole"]);
    }

    [Fact]
    public async Task DeleteRole_NotFound_ReturnsGraphQLError()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(KernelResult.Failure("ROLE_NOT_FOUND", "Role not found."));

        var executor = await BuildExecutorAsync(sender);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteRole(id: \"{Guid.NewGuid()}\") }}"))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.Equal("ROLE_NOT_FOUND", result.Errors![0].Extensions?["code"]?.ToString());
    }

    [Fact]
    public async Task DeleteRole_WithoutToken_ReturnsAuthorizationError()
    {
        var executor = await BuildExecutorAsync(Substitute.For<ISender>());
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteRole(id: \"{Guid.NewGuid()}\") }}",
            authenticated: false))).ExpectOperationResult();

        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }
}

[ExtendObjectType(OperationTypeNames.Query)]
internal sealed class RoleTestQuery
{
    public string Ping() => "pong";
}
