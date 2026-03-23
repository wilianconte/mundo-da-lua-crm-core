using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// Course is the master entity representing any educational offering or structured program.
///
/// It is intentionally generic to support multiple business contexts:
///   - After-school reinforcement programs
///   - Language courses (English, Spanish, etc.)
///   - School class groups (turmas)
///   - Short workshops or events
///   - Any other structured educational/service program with a beginning and an end
///
/// Design decisions:
///   - Course is the source of truth for program identity and master data.
///   - Student-specific enrollment data (dates, status, group, shift) is stored in StudentCourse.
///   - Course does NOT store student lists directly — navigation is through StudentCourse.
///   - All identity and personal data for participants remains in Person/Student.
///
/// Relationships:
///   Course 1──0..* StudentCourse  (FK on StudentCourse.CourseId)
/// </summary>
public sealed class Course : TenantEntity
{
    // ── Identity ──────────────────────────────────────────────────────────────

    /// <summary>Human-readable name of the course or program (e.g. "English Level A1 - 2025/1").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Short unique code for the course within the tenant (e.g. "ENG-A1-2025-1").
    /// Useful for internal reference and integrations.
    /// </summary>
    public string? Code { get; private set; }

    /// <summary>Category of the course. Drives business rules and UI presentation.</summary>
    public CourseType Type { get; private set; }

    /// <summary>Optional detailed description of the course content and objectives.</summary>
    public string? Description { get; private set; }

    // ── Schedule ──────────────────────────────────────────────────────────────

    /// <summary>Date on which the course starts.</summary>
    public DateOnly? StartDate { get; private set; }

    /// <summary>Date on which the course ends or is expected to end.</summary>
    public DateOnly? EndDate { get; private set; }

    /// <summary>
    /// Free-text description of the weekly schedule (e.g. "Monday and Wednesday, 14h–16h").
    /// Structured scheduling can be added via a dedicated Schedule entity in the future.
    /// </summary>
    public string? ScheduleDescription { get; private set; }

    // ── Capacity & Workload ───────────────────────────────────────────────────

    /// <summary>Maximum number of students allowed in this course. Null means no limit defined.</summary>
    public int? Capacity { get; private set; }

    /// <summary>Total hours of the course (carga horária). Null if not applicable.</summary>
    public int? Workload { get; private set; }

    // ── Unit & Context ────────────────────────────────────────────────────────

    /// <summary>Reference to the unit/branch that offers this course. Null means all units or not applicable.</summary>
    public Guid? UnitId { get; private set; }

    // ── Status & Flags ────────────────────────────────────────────────────────

    /// <summary>Lifecycle status of the course (Draft, Active, Inactive, Completed, Cancelled).</summary>
    public CourseStatus Status { get; private set; }

    /// <summary>
    /// Convenience flag for quick active/inactive filtering without checking Status details.
    /// Automatically managed by domain methods.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>Internal notes about the course (not visible to students).</summary>
    public string? Notes { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>All student enrollments associated with this course. Navigation property for EF Core.</summary>
    public ICollection<StudentCourse> Enrollments { get; private set; } = [];

    // ─────────────────────────────────────────────────────────────────────────

    private Course() { }

    /// <summary>
    /// Factory method — the only way to create a new Course.
    /// Enforces required fields and sets default lifecycle state.
    /// </summary>
    public static Course Create(
        Guid tenantId,
        string name,
        CourseType type,
        string? code                 = null,
        string? description          = null,
        DateOnly? startDate          = null,
        DateOnly? endDate            = null,
        string? scheduleDescription  = null,
        int? capacity                = null,
        int? workload                = null,
        Guid? unitId                 = null,
        string? notes                = null,
        CourseStatus status          = CourseStatus.Draft)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name is required.", nameof(name));

        return new Course
        {
            TenantId            = tenantId,
            Name                = name.Trim(),
            Code                = code?.Trim(),
            Type                = type,
            Description         = description?.Trim(),
            StartDate           = startDate,
            EndDate             = endDate,
            ScheduleDescription = scheduleDescription?.Trim(),
            Capacity            = capacity,
            Workload            = workload,
            UnitId              = unitId,
            Status              = status,
            IsActive            = status == CourseStatus.Active,
            Notes               = notes?.Trim(),
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void UpdateInfo(
        string name,
        CourseType type,
        string? code,
        string? description,
        DateOnly? startDate,
        DateOnly? endDate,
        string? scheduleDescription,
        int? capacity,
        int? workload,
        Guid? unitId,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name is required.", nameof(name));

        Name                = name.Trim();
        Code                = code?.Trim();
        Type                = type;
        Description         = description?.Trim();
        StartDate           = startDate;
        EndDate             = endDate;
        ScheduleDescription = scheduleDescription?.Trim();
        Capacity            = capacity;
        Workload            = workload;
        UnitId              = unitId;
        Notes               = notes?.Trim();
        Touch();
    }

    public void ChangeStatus(CourseStatus newStatus)
    {
        Status   = newStatus;
        IsActive = newStatus == CourseStatus.Active;
        Touch();
    }

    public void Activate()
    {
        Status   = CourseStatus.Active;
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        Status   = CourseStatus.Inactive;
        IsActive = false;
        Touch();
    }

    public void Complete()
    {
        Status   = CourseStatus.Completed;
        IsActive = false;
        Touch();
    }

    public void Cancel()
    {
        Status   = CourseStatus.Cancelled;
        IsActive = false;
        Touch();
    }

    public void Publish()
    {
        Status   = CourseStatus.Active;
        IsActive = true;
        Touch();
    }
}
