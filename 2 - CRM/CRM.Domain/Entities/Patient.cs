using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class Patient : TenantEntity
{
    public Guid PersonId { get; private set; }
    public PatientStatus Status { get; private set; }
    public string? Notes { get; private set; }

    // EF Core navigation — do not use in domain logic
    public Person? Person { get; private set; }

    private Patient() { }

    public static Patient Create(Guid tenantId, Guid personId, string? notes = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("PersonId is required.", nameof(personId));

        return new Patient
        {
            TenantId = tenantId,
            PersonId = personId,
            Status = PatientStatus.Active,
            Notes = notes?.Trim()
        };
    }

    public void UpdateNotes(string? notes) { Notes = notes?.Trim(); Touch(); }
    public void Activate() { Status = PatientStatus.Active; Touch(); }
    public void Deactivate() { Status = PatientStatus.Inactive; Touch(); }
    public void Block() { Status = PatientStatus.Blocked; Touch(); }
}
