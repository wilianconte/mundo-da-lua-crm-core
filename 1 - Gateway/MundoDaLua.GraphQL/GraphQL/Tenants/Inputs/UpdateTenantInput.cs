using MyCRM.Auth.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Tenants.Inputs;

public record UpdateTenantInput(
    string Name,
    TenantPlan Plan,
    TenantStatus Status);
