using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Tenants.RegisterTenant;

/// <summary>
/// Registra um novo tenant criando em uma única operação:
///   - Company (CRM)
///   - Person / administrador (CRM)
///   - Tenant (Auth)
///   - User de acesso (Auth)
///   - Role "Administrador" para o tenant (Auth)
/// </summary>
public record RegisterTenantCommand(
    // ── Empresa ──────────────────────────────────────────────
    string CompanyLegalName,
    string? CompanyCnpj,
    string? CompanyEmail,
    string? CompanyPhone,

    // ── Pessoa / administrador ────────────────────────────────
    string AdminName,
    string AdminEmail,
    string? AdminCpf,
    string? AdminPhone,

    // ── Acesso ───────────────────────────────────────────────
    string Password,
    string PasswordConfirmation
) : IRequest<Result<TenantDto>>;
