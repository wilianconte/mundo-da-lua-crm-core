using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class Appointment : TenantEntity
{
    public Guid ProfessionalId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public AppointmentType Type { get; private set; }
    public decimal Price { get; private set; }
    public Address? Address { get; private set; }
    public string? MeetingLink { get; private set; }
    public PaymentReceiverType PaymentReceiver { get; private set; }
    public Guid PaymentMethodId { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public Guid? RecurrenceId { get; private set; }
    public string? ConfirmedBy { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public bool CancelledWithLateNotice { get; private set; }
    public DateTime? NoShowAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? RescheduledFrom { get; private set; }
    public DateTime? RescheduledAt { get; private set; }
    public Guid? TransactionId { get; private set; }
    public string? Notes { get; private set; }

    // EF Core navigation — do not use in domain logic
    public Professional? Professional { get; private set; }
    public Patient? Patient { get; private set; }
    public Service? Service { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    public AppointmentRecurrence? Recurrence { get; private set; }

    private Appointment() { }

    public static Appointment Create(
        Guid tenantId,
        Guid professionalId,
        Guid patientId,
        Guid serviceId,
        DateTime startDateTime,
        DateTime endDateTime,
        AppointmentType type,
        decimal price,
        PaymentReceiverType paymentReceiver,
        Guid paymentMethodId,
        Address? address = null,
        string? meetingLink = null,
        Guid? recurrenceId = null,
        Guid? rescheduledFrom = null,
        string? notes = null)
    {
        if (professionalId == Guid.Empty)
            throw new ArgumentException("ProfessionalId is required.", nameof(professionalId));
        if (patientId == Guid.Empty)
            throw new ArgumentException("PatientId is required.", nameof(patientId));
        if (serviceId == Guid.Empty)
            throw new ArgumentException("ServiceId is required.", nameof(serviceId));
        if (paymentMethodId == Guid.Empty)
            throw new ArgumentException("PaymentMethodId is required.", nameof(paymentMethodId));
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero.", nameof(price));
        if (endDateTime <= startDateTime)
            throw new ArgumentException("EndDateTime must be greater than StartDateTime.", nameof(endDateTime));

        return new Appointment
        {
            TenantId = tenantId,
            ProfessionalId = professionalId,
            PatientId = patientId,
            ServiceId = serviceId,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            Type = type,
            Price = price,
            Address = address,
            MeetingLink = meetingLink?.Trim(),
            PaymentReceiver = paymentReceiver,
            PaymentMethodId = paymentMethodId,
            Status = AppointmentStatus.Suggested,
            RecurrenceId = recurrenceId,
            RescheduledFrom = rescheduledFrom,
            RescheduledAt = rescheduledFrom.HasValue ? DateTime.UtcNow : null,
            Notes = notes?.Trim()
        };
    }

    public void Confirm(string confirmedBy)
    {
        Status = AppointmentStatus.Confirmed;
        ConfirmedBy = confirmedBy?.Trim();
        ConfirmedAt = DateTime.UtcNow;
        Touch();
    }

    public void Complete(Guid? transactionId = null)
    {
        Status = AppointmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        TransactionId = transactionId;
        Touch();
    }

    public void Cancel(string? reason, bool lateNotice = false)
    {
        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason?.Trim();
        CancelledAt = DateTime.UtcNow;
        CancelledWithLateNotice = lateNotice;
        Touch();
    }

    public void MarkNoShow()
    {
        Status = AppointmentStatus.NoShow;
        NoShowAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkRescheduled()
    {
        Status = AppointmentStatus.Rescheduled;
        Touch();
    }

    public void LinkTransaction(Guid transactionId)
    {
        TransactionId = transactionId;
        Touch();
    }

    public void UpdateNotes(string? notes) { Notes = notes?.Trim(); Touch(); }
}
