using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class ProfessionalService : TenantEntity
{
    public Guid ProfessionalId { get; private set; }
    public Guid ServiceId { get; private set; }
    public decimal? CustomPrice { get; private set; }
    public int? CustomDurationInMinutes { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core navigation — do not use in domain logic
    public Professional? Professional { get; private set; }
    public Service? Service { get; private set; }

    private ProfessionalService() { }

    public static ProfessionalService Create(Guid tenantId, Guid professionalId, Guid serviceId, decimal? customPrice = null, int? customDurationInMinutes = null)
    {
        if (professionalId == Guid.Empty)
            throw new ArgumentException("ProfessionalId is required.", nameof(professionalId));
        if (serviceId == Guid.Empty)
            throw new ArgumentException("ServiceId is required.", nameof(serviceId));
        if (customPrice.HasValue && customPrice <= 0)
            throw new ArgumentException("CustomPrice must be greater than zero.", nameof(customPrice));
        if (customDurationInMinutes.HasValue && customDurationInMinutes <= 0)
            throw new ArgumentException("CustomDurationInMinutes must be greater than zero.", nameof(customDurationInMinutes));

        return new ProfessionalService
        {
            TenantId = tenantId,
            ProfessionalId = professionalId,
            ServiceId = serviceId,
            CustomPrice = customPrice,
            CustomDurationInMinutes = customDurationInMinutes,
            IsActive = true
        };
    }

    public void Update(decimal? customPrice, int? customDurationInMinutes)
    {
        if (customPrice.HasValue && customPrice <= 0)
            throw new ArgumentException("CustomPrice must be greater than zero.", nameof(customPrice));
        if (customDurationInMinutes.HasValue && customDurationInMinutes <= 0)
            throw new ArgumentException("CustomDurationInMinutes must be greater than zero.", nameof(customDurationInMinutes));

        CustomPrice = customPrice;
        CustomDurationInMinutes = customDurationInMinutes;
        Touch();
    }

    public void Activate() { IsActive = true; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }
}
