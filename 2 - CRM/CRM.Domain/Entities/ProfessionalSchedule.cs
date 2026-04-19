using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class ProfessionalSchedule : TenantEntity
{
    public Guid ProfessionalId { get; private set; }
    public int DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public bool IsAvailable { get; private set; }

    // EF Core navigation — do not use in domain logic
    public Professional? Professional { get; private set; }

    private ProfessionalSchedule() { }

    public static ProfessionalSchedule Create(Guid tenantId, Guid professionalId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, bool isAvailable = true)
    {
        if (professionalId == Guid.Empty)
            throw new ArgumentException("ProfessionalId is required.", nameof(professionalId));
        if (dayOfWeek < 0 || dayOfWeek > 6)
            throw new ArgumentException("DayOfWeek must be between 0 (Sunday) and 6 (Saturday).", nameof(dayOfWeek));
        if (endTime <= startTime)
            throw new ArgumentException("EndTime must be greater than StartTime.", nameof(endTime));

        return new ProfessionalSchedule
        {
            TenantId = tenantId,
            ProfessionalId = professionalId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            IsAvailable = isAvailable
        };
    }

    public void Update(TimeSpan startTime, TimeSpan endTime, bool isAvailable)
    {
        if (endTime <= startTime)
            throw new ArgumentException("EndTime must be greater than StartTime.", nameof(endTime));

        StartTime = startTime;
        EndTime = endTime;
        IsAvailable = isAvailable;
        Touch();
    }
}
