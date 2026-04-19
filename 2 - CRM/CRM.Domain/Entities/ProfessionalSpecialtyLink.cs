using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class ProfessionalSpecialtyLink : TenantEntity
{
    public Guid ProfessionalId { get; private set; }
    public Guid SpecialtyId { get; private set; }

    // EF Core navigation — do not use in domain logic
    public Professional? Professional { get; private set; }
    public ProfessionalSpecialty? Specialty { get; private set; }

    private ProfessionalSpecialtyLink() { }

    public static ProfessionalSpecialtyLink Create(Guid tenantId, Guid professionalId, Guid specialtyId)
    {
        if (professionalId == Guid.Empty)
            throw new ArgumentException("ProfessionalId is required.", nameof(professionalId));
        if (specialtyId == Guid.Empty)
            throw new ArgumentException("SpecialtyId is required.", nameof(specialtyId));

        return new ProfessionalSpecialtyLink
        {
            TenantId = tenantId,
            ProfessionalId = professionalId,
            SpecialtyId = specialtyId
        };
    }
}
