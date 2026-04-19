using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class Professional : TenantEntity
{
    public Guid PersonId { get; private set; }
    public decimal? CommissionPercentage { get; private set; }
    public ProfessionalStatus Status { get; private set; }
    public string? Bio { get; private set; }
    public string? LicenseNumber { get; private set; }
    public Guid? WalletId { get; private set; }

    // EF Core navigation — do not use in domain logic
    public Person? Person { get; private set; }
    public Wallet? Wallet { get; private set; }

    private Professional() { }

    public static Professional Create(Guid tenantId, Guid personId, string? bio = null, string? licenseNumber = null, decimal? commissionPercentage = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("PersonId is required.", nameof(personId));

        return new Professional
        {
            TenantId = tenantId,
            PersonId = personId,
            Status = ProfessionalStatus.Draft,
            Bio = bio?.Trim(),
            LicenseNumber = licenseNumber?.Trim(),
            CommissionPercentage = commissionPercentage
        };
    }

    public void Update(string? bio, string? licenseNumber, decimal? commissionPercentage)
    {
        Bio = bio?.Trim();
        LicenseNumber = licenseNumber?.Trim();
        CommissionPercentage = commissionPercentage;
        Touch();
    }

    // Wallet criada automaticamente ao ativar Draft → Active (RN-061)
    public void Activate(Guid walletId)
    {
        if (walletId == Guid.Empty)
            throw new ArgumentException("WalletId is required to activate a Professional (RN-061).", nameof(walletId));
        Status = ProfessionalStatus.Active;
        WalletId = walletId;
        Touch();
    }

    public void Deactivate() { Status = ProfessionalStatus.Inactive; Touch(); }
    public void Suspend() { Status = ProfessionalStatus.Suspended; Touch(); }
    public void Terminate() { Status = ProfessionalStatus.Terminated; SoftDelete(); Touch(); }
}
