using HotChocolate.Execution.Configuration;
using MyCRM.GraphQL.GraphQL.Auth;
using MyCRM.GraphQL.GraphQL.Plans;
using MyCRM.GraphQL.GraphQL.Tenants;

namespace MyCRM.GraphQL.Extensions;

public static class AuthGraphQLExtensions
{
    public static IRequestExecutorBuilder AddAuthGraphQL(this IRequestExecutorBuilder builder) => builder
        // Auth (users, roles, permissions)
        .AddTypeExtension<AuthMutations>()
        .AddTypeExtension<UserQueries>()
        .AddType<UserObjectType>()
        .AddTypeExtension<RoleQueries>()
        .AddTypeExtension<RoleMutations>()
        .AddType<RoleObjectType>()
        .AddTypeExtension<PermissionQueries>()
        .AddTypeExtension<PermissionAdminQueries>()
        // Tenants
        .AddTypeExtension<TenantQueries>()
        .AddTypeExtension<TenantMutations>()
        // Plans / Subscriptions
        .AddTypeExtension<TenantPlanMutations>();
}
