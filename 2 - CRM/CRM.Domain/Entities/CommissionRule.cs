using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class CommissionRule : TenantEntity
{
    public Guid? ProfessionalId { get; private set; }
    public Guid? ServiceId { get; private set; }
    public decimal CompanyPercentage { get; private set; }

    // EF Core navigation — do not use in domain logic
    public Professional? Professional { get; private set; }
    public Service? Service { get; private set; }

    private CommissionRule() { }

    public static CommissionRule Create(Guid tenantId, decimal companyPercentage, Guid? professionalId = null, Guid? serviceId = null)
    {
        if (companyPercentage < 0 || companyPercentage > 100)
            throw new ArgumentException("CompanyPercentage must be between 0 and 100.", nameof(companyPercentage));

        return new CommissionRule
        {
            TenantId = tenantId,
            ProfessionalId = professionalId,
            ServiceId = serviceId,
            CompanyPercentage = companyPercentage
        };
    }

    public void Update(decimal companyPercentage)
    {
        if (companyPercentage < 0 || companyPercentage > 100)
            throw new ArgumentException("CompanyPercentage must be between 0 and 100.", nameof(companyPercentage));

        CompanyPercentage = companyPercentage;
        Touch();
    }
}
