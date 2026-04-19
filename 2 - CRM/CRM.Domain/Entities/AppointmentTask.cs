using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class AppointmentTask : TenantEntity
{
    public Guid AppointmentId { get; private set; }
    public AppointmentTaskType Type { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public string? AssignedToRole { get; private set; }
    public AppointmentTaskStatus Status { get; private set; }
    public string? Result { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    // EF Core navigation — do not use in domain logic
    public Appointment? Appointment { get; private set; }

    private AppointmentTask() { }

    public static AppointmentTask Create(Guid tenantId, Guid appointmentId, AppointmentTaskType type, Guid? assignedToUserId = null, string? assignedToRole = null)
    {
        if (appointmentId == Guid.Empty)
            throw new ArgumentException("AppointmentId is required.", nameof(appointmentId));

        return new AppointmentTask
        {
            TenantId = tenantId,
            AppointmentId = appointmentId,
            Type = type,
            AssignedToUserId = assignedToUserId,
            AssignedToRole = assignedToRole?.Trim(),
            Status = AppointmentTaskStatus.Pending
        };
    }

    public void Complete(string? result)
    {
        Status = AppointmentTaskStatus.Completed;
        Result = result?.Trim();
        ResolvedAt = DateTime.UtcNow;
        Touch();
    }

    public void Escalate()
    {
        Status = AppointmentTaskStatus.Escalated;
        ResolvedAt = DateTime.UtcNow;
        Touch();
    }

    public void Expire()
    {
        Status = AppointmentTaskStatus.Expired;
        ResolvedAt = DateTime.UtcNow;
        Touch();
    }
}
