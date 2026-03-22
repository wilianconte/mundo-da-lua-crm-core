using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// StudentGuardian represents the relationship between a Student and a responsible Person.
///
/// This is a relationship entity — it stores business attributes of the
/// guardian/student bond rather than duplicating any identity data.
/// The responsible party is represented directly by Person (via GuardianPersonId),
/// so there is no separate Guardian entity required.
///
/// Relationships:
///   StudentGuardian *──1 Student  (FK on StudentGuardian.StudentId)
///   StudentGuardian *──1 Person   (FK on StudentGuardian.GuardianPersonId)
///
/// Uniqueness constraint: (StudentId + GuardianPersonId) must be unique per tenant
/// to prevent the same guardian from being added twice for the same student.
///
/// Only one primary guardian should exist per student (enforced by business logic).
/// </summary>
public sealed class StudentGuardian : TenantEntity
{
    // ── References ────────────────────────────────────────────────────────────

    /// <summary>The student this guardian is responsible for.</summary>
    public Guid StudentId { get; private set; }

    /// <summary>Navigation property for EF Core.</summary>
    public Student? Student { get; private set; }

    /// <summary>
    /// The Person acting as guardian/responsible party.
    /// References the master Person entity — no personal data is stored here.
    /// </summary>
    public Guid GuardianPersonId { get; private set; }

    /// <summary>Navigation property for EF Core.</summary>
    public Person? GuardianPerson { get; private set; }

    // ── Relationship Attributes ───────────────────────────────────────────────

    public GuardianRelationshipType RelationshipType { get; private set; }

    /// <summary>Indicates this is the main/primary responsible person for the student.</summary>
    public bool IsPrimaryGuardian { get; private set; }

    /// <summary>Indicates this guardian is financially responsible (receives invoices/contracts).</summary>
    public bool IsFinancialResponsible { get; private set; }

    /// <summary>Indicates this guardian should receive notifications (messages, alerts, reports).</summary>
    public bool ReceivesNotifications { get; private set; }

    /// <summary>Indicates this guardian is authorized to pick up the child.</summary>
    public bool CanPickupChild { get; private set; }

    public string? Notes { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────

    private StudentGuardian() { }

    /// <summary>
    /// Factory method — the only way to create a new StudentGuardian relationship.
    /// Duplicate prevention (same guardianPersonId + studentId) must be enforced
    /// at the application layer before calling this method.
    /// </summary>
    public static StudentGuardian Create(
        Guid tenantId,
        Guid studentId,
        Guid guardianPersonId,
        GuardianRelationshipType relationshipType,
        bool isPrimaryGuardian       = false,
        bool isFinancialResponsible  = false,
        bool receivesNotifications   = true,
        bool canPickupChild          = false,
        string? notes                = null)
    {
        if (studentId == Guid.Empty)
            throw new ArgumentException("StudentId is required.", nameof(studentId));

        if (guardianPersonId == Guid.Empty)
            throw new ArgumentException("GuardianPersonId is required.", nameof(guardianPersonId));

        return new StudentGuardian
        {
            TenantId              = tenantId,
            StudentId             = studentId,
            GuardianPersonId      = guardianPersonId,
            RelationshipType      = relationshipType,
            IsPrimaryGuardian     = isPrimaryGuardian,
            IsFinancialResponsible = isFinancialResponsible,
            ReceivesNotifications = receivesNotifications,
            CanPickupChild        = canPickupChild,
            Notes                 = notes?.Trim(),
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void UpdateRelationship(
        GuardianRelationshipType relationshipType,
        bool isPrimaryGuardian,
        bool isFinancialResponsible,
        bool receivesNotifications,
        bool canPickupChild,
        string? notes)
    {
        RelationshipType       = relationshipType;
        IsPrimaryGuardian      = isPrimaryGuardian;
        IsFinancialResponsible = isFinancialResponsible;
        ReceivesNotifications  = receivesNotifications;
        CanPickupChild         = canPickupChild;
        Notes                  = notes?.Trim();
        Touch();
    }

    public void SetAsPrimaryGuardian()
    {
        IsPrimaryGuardian = true;
        Touch();
    }

    public void UnsetPrimaryGuardian()
    {
        IsPrimaryGuardian = false;
        Touch();
    }
}
