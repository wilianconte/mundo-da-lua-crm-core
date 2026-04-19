using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class AppointmentRecurrence : TenantEntity
{
    public RecurrenceFrequency Frequency { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public int? MaxOccurrences { get; private set; }
    public Guid ParentAppointmentId { get; private set; }
    public int CreatedOccurrences { get; private set; }

    private AppointmentRecurrence() { }

    public static AppointmentRecurrence Create(Guid tenantId, Guid parentAppointmentId, RecurrenceFrequency frequency, DateOnly? endDate = null, int? maxOccurrences = null)
    {
        if (parentAppointmentId == Guid.Empty)
            throw new ArgumentException("ParentAppointmentId is required.", nameof(parentAppointmentId));
        if (endDate == null && maxOccurrences == null)
            throw new ArgumentException("Either EndDate or MaxOccurrences must be provided (RN-057).");

        return new AppointmentRecurrence
        {
            TenantId = tenantId,
            ParentAppointmentId = parentAppointmentId,
            Frequency = frequency,
            EndDate = endDate,
            MaxOccurrences = maxOccurrences,
            CreatedOccurrences = 0
        };
    }

    public void IncrementOccurrences() { CreatedOccurrences++; Touch(); }
}
