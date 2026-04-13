namespace MyCRM.GraphQL.GraphQL.Tenants.Inputs;

/// <summary>
/// Dados mínimos para registrar um novo tenant.
/// Cria em uma única operação: Empresa (CRM) + Pessoa admin (CRM) + Tenant + Usuário de acesso (Auth).
/// </summary>
public record RegisterTenantInput(
    // ── Empresa ──────────────────────────────────────────────────────────────
    string CompanyLegalName,
    string? CompanyCnpj,
    string? CompanyEmail,
    string? CompanyPhone,

    // ── Pessoa / administrador ────────────────────────────────────────────────
    string AdminName,
    string AdminEmail,
    string? AdminCpf,
    string? AdminPhone,

    // ── Acesso ───────────────────────────────────────────────────────────────
    string Password,
    string PasswordConfirmation);
