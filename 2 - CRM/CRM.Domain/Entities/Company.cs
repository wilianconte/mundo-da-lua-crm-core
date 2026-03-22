using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// Company is the canonical master entity for all legal entities and organizations
/// in the MyCRM platform.
///
/// A single real-world organization (e.g. a school, supplier, or corporate client)
/// must have exactly one Company record. All role-specific data lives in dedicated
/// satellite entities that reference Company by CompanyId, preventing duplicate
/// company records across modules.
///
/// Planned relationships (FK defined by each role entity, not by Company):
///   Company 1──0..1  Supplier          (FK on Supplier.CompanyId)
///   Company 1──0..1  Partner           (FK on Partner.CompanyId)
///   Company 1──0..1  School            (FK on School.CompanyId)
///   Company 1──0..1  CorporateCustomer (FK on CorporateCustomer.CompanyId)
///   Company 1──0..1  BillingAccount    (FK on BillingAccount.CompanyId)
///   Company 1──0..1  ServiceProvider   (FK on ServiceProvider.CompanyId)
///
/// Deduplication strategy:
///   - RegistrationNumber (CNPJ) is unique per tenant (partial index, nullable).
///   - Email is unique per tenant (partial index, nullable).
///   - Before creating a Company, check both fields via ICompanyRepository.
/// </summary>
public sealed class Company : TenantEntity
{
    // ── Identity ─────────────────────────────────────────────────────────────

    /// <summary>Legal/corporate name as registered in official documents (Razão Social).</summary>
    public string LegalName { get; private set; } = default!;

    /// <summary>Trade name / brand name used in day-to-day operations (Nome Fantasia).</summary>
    public string? TradeName { get; private set; }

    /// <summary>
    /// National company registration number (CNPJ for Brazil).
    /// Primary deduplication key — unique per tenant when provided.
    /// </summary>
    public string? RegistrationNumber { get; private set; }

    /// <summary>State tax registration number (Inscrição Estadual).</summary>
    public string? StateRegistration { get; private set; }

    /// <summary>Municipal tax registration number (Inscrição Municipal).</summary>
    public string? MunicipalRegistration { get; private set; }

    // ── Contact ───────────────────────────────────────────────────────────────

    /// <summary>Primary corporate email. Unique per tenant when provided. Stored lowercase.</summary>
    public string? Email { get; private set; }

    public string? PrimaryPhone { get; private set; }

    public string? SecondaryPhone { get; private set; }

    /// <summary>WhatsApp number — may differ from PrimaryPhone.</summary>
    public string? WhatsAppNumber { get; private set; }

    public string? Website { get; private set; }

    // ── Contact Person ────────────────────────────────────────────────────────

    /// <summary>Name of the primary human contact at this company.</summary>
    public string? ContactPersonName { get; private set; }

    /// <summary>Email of the primary contact person.</summary>
    public string? ContactPersonEmail { get; private set; }

    /// <summary>Phone of the primary contact person.</summary>
    public string? ContactPersonPhone { get; private set; }

    // ── Classification ────────────────────────────────────────────────────────

    /// <summary>
    /// Primary classification of the company within the platform.
    /// Role-specific data is stored in satellite entities, not here.
    /// </summary>
    public CompanyType? CompanyType { get; private set; }

    /// <summary>Industry or market segment (e.g. "Education", "Healthcare", "Technology").</summary>
    public string? Industry { get; private set; }

    // ── Profile ───────────────────────────────────────────────────────────────

    public string? ProfileImageUrl { get; private set; }

    // ── Address ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Primary address of the company. Stored as an owned value object,
    /// consistent with the existing Address pattern used by Customer.
    /// </summary>
    public Address? Address { get; private set; }

    // ── Status & Notes ────────────────────────────────────────────────────────

    public CompanyStatus Status { get; private set; }

    public string? Notes { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────

    private Company() { }

    /// <summary>
    /// Factory method — the only way to create a new Company.
    /// TenantId is also set here explicitly; the DbContext SaveChangesAsync
    /// interceptor reinforces it on INSERT.
    /// </summary>
    public static Company Create(
        Guid tenantId,
        string legalName,
        string? tradeName = null,
        string? registrationNumber = null,
        string? stateRegistration = null,
        string? municipalRegistration = null,
        string? email = null,
        string? primaryPhone = null,
        string? secondaryPhone = null,
        string? whatsAppNumber = null,
        string? website = null,
        string? contactPersonName = null,
        string? contactPersonEmail = null,
        string? contactPersonPhone = null,
        CompanyType? companyType = null,
        string? industry = null,
        string? profileImageUrl = null,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(legalName);

        return new Company
        {
            TenantId             = tenantId,
            LegalName            = legalName.Trim(),
            TradeName            = tradeName?.Trim(),
            RegistrationNumber   = registrationNumber?.Trim(),
            StateRegistration    = stateRegistration?.Trim(),
            MunicipalRegistration = municipalRegistration?.Trim(),
            Email                = email?.Trim().ToLowerInvariant(),
            PrimaryPhone         = primaryPhone?.Trim(),
            SecondaryPhone       = secondaryPhone?.Trim(),
            WhatsAppNumber       = whatsAppNumber?.Trim(),
            Website              = website?.Trim(),
            ContactPersonName    = contactPersonName?.Trim(),
            ContactPersonEmail   = contactPersonEmail?.Trim().ToLowerInvariant(),
            ContactPersonPhone   = contactPersonPhone?.Trim(),
            CompanyType          = companyType,
            Industry             = industry?.Trim(),
            ProfileImageUrl      = profileImageUrl?.Trim(),
            Notes                = notes?.Trim(),
            Status               = CompanyStatus.Active,
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void UpdateProfile(
        string legalName,
        string? tradeName,
        string? stateRegistration,
        string? municipalRegistration,
        CompanyType? companyType,
        string? industry,
        string? profileImageUrl,
        string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(legalName);
        LegalName             = legalName.Trim();
        TradeName             = tradeName?.Trim();
        StateRegistration     = stateRegistration?.Trim();
        MunicipalRegistration = municipalRegistration?.Trim();
        CompanyType           = companyType;
        Industry              = industry?.Trim();
        ProfileImageUrl       = profileImageUrl?.Trim();
        Notes                 = notes?.Trim();
        Touch();
    }

    public void UpdateContact(
        string? email,
        string? primaryPhone,
        string? secondaryPhone,
        string? whatsAppNumber,
        string? website,
        string? contactPersonName,
        string? contactPersonEmail,
        string? contactPersonPhone)
    {
        Email                = email?.Trim().ToLowerInvariant();
        PrimaryPhone         = primaryPhone?.Trim();
        SecondaryPhone       = secondaryPhone?.Trim();
        WhatsAppNumber       = whatsAppNumber?.Trim();
        Website              = website?.Trim();
        ContactPersonName    = contactPersonName?.Trim();
        ContactPersonEmail   = contactPersonEmail?.Trim().ToLowerInvariant();
        ContactPersonPhone   = contactPersonPhone?.Trim();
        Touch();
    }

    public void UpdateRegistrationNumber(string? registrationNumber)
    {
        RegistrationNumber = registrationNumber?.Trim();
        Touch();
    }

    public void SetAddress(Address address)
    {
        Address = address;
        Touch();
    }

    public void Activate()   { Status = CompanyStatus.Active;    Touch(); }
    public void Deactivate() { Status = CompanyStatus.Inactive;  Touch(); }
    public void Block()      { Status = CompanyStatus.Blocked;   Touch(); }
    public void Suspend()    { Status = CompanyStatus.Suspended; Touch(); }
}
