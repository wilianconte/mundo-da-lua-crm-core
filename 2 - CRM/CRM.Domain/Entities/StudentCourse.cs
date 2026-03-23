using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// StudentCourse is the association/enrollment entity linking a Student to a Course.
///
/// This entity stores enrollment-specific data only — it does NOT duplicate student
/// identity data (stored in Person/Student) or course master data (stored in Course).
///
/// Design decisions:
///   - Historical re-enrollment is supported: the same student may enroll in the same
///     course more than once over time (e.g. repeated a year, returned after transfer).
///   - To prevent simultaneous duplicate active enrollments, a unique partial index on
///     (TenantId, StudentId, CourseId) is applied where IsDeleted = false and
///     Status = Active/Pending. Business-layer validation enforces this rule.
///   - All enrollment lifecycle attributes (cancellation, completion, transfer) are
///     tracked within this entity, keeping Course and Student free of enrollment state.
///
/// Relationships:
///   StudentCourse *──1 Student  (FK on StudentCourse.StudentId)
///   StudentCourse *──1 Course   (FK on StudentCourse.CourseId)
/// </summary>
public sealed class StudentCourse : TenantEntity
{
    // ── References ────────────────────────────────────────────────────────────

    /// <summary>The enrolled student. References the Student role entity.</summary>
    public Guid StudentId { get; private set; }

    /// <summary>Navigation property for EF Core — do not use directly in domain logic.</summary>
    public Student? Student { get; private set; }

    /// <summary>The course the student is enrolled in.</summary>
    public Guid CourseId { get; private set; }

    /// <summary>Navigation property for EF Core — do not use directly in domain logic.</summary>
    public Course? Course { get; private set; }

    // ── Enrollment Lifecycle ──────────────────────────────────────────────────

    /// <summary>Date when the enrollment was registered in the system.</summary>
    public DateOnly? EnrollmentDate { get; private set; }

    /// <summary>Date when the student actually started attending the course.</summary>
    public DateOnly? StartDate { get; private set; }

    /// <summary>Date when the student's participation ended or is expected to end.</summary>
    public DateOnly? EndDate { get; private set; }

    /// <summary>Date when the enrollment was cancelled, if applicable.</summary>
    public DateOnly? CancellationDate { get; private set; }

    /// <summary>Date when the student completed the course, if applicable.</summary>
    public DateOnly? CompletionDate { get; private set; }

    // ── Enrollment Status ─────────────────────────────────────────────────────

    /// <summary>Current lifecycle status of the enrollment.</summary>
    public StudentCourseStatus Status { get; private set; }

    // ── Context ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Class group or section within the course (e.g. "Turma A", "Group 2").
    /// May differ from the Course's general schedule if the student has a specific group.
    /// </summary>
    public string? ClassGroup { get; private set; }

    /// <summary>Shift (e.g. "Morning", "Afternoon", "Evening") for this enrollment.</summary>
    public string? Shift { get; private set; }

    /// <summary>
    /// Enrollment-specific schedule description, if different from the Course's schedule.
    /// </summary>
    public string? ScheduleDescription { get; private set; }

    /// <summary>Reference to the unit/branch where this specific enrollment takes place.</summary>
    public Guid? UnitId { get; private set; }

    /// <summary>Internal notes about this enrollment (e.g. special needs, accommodation).</summary>
    public string? Notes { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────

    private StudentCourse() { }

    /// <summary>
    /// Factory method — the only way to create a new StudentCourse enrollment.
    /// Duplicate active enrollment prevention must be enforced at the application layer
    /// before calling this method.
    /// </summary>
    public static StudentCourse Create(
        Guid tenantId,
        Guid studentId,
        Guid courseId,
        DateOnly? enrollmentDate       = null,
        DateOnly? startDate            = null,
        DateOnly? endDate              = null,
        string? classGroup             = null,
        string? shift                  = null,
        string? scheduleDescription    = null,
        Guid? unitId                   = null,
        string? notes                  = null)
    {
        if (studentId == Guid.Empty)
            throw new ArgumentException("StudentId is required.", nameof(studentId));

        if (courseId == Guid.Empty)
            throw new ArgumentException("CourseId is required.", nameof(courseId));

        return new StudentCourse
        {
            TenantId            = tenantId,
            StudentId           = studentId,
            CourseId            = courseId,
            EnrollmentDate      = enrollmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            StartDate           = startDate,
            EndDate             = endDate,
            Status              = StudentCourseStatus.Pending,
            ClassGroup          = classGroup?.Trim(),
            Shift               = shift?.Trim(),
            ScheduleDescription = scheduleDescription?.Trim(),
            UnitId              = unitId,
            Notes               = notes?.Trim(),
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void UpdateInfo(
        DateOnly? enrollmentDate,
        DateOnly? startDate,
        DateOnly? endDate,
        string? classGroup,
        string? shift,
        string? scheduleDescription,
        Guid? unitId,
        string? notes)
    {
        EnrollmentDate      = enrollmentDate;
        StartDate           = startDate;
        EndDate             = endDate;
        ClassGroup          = classGroup?.Trim();
        Shift               = shift?.Trim();
        ScheduleDescription = scheduleDescription?.Trim();
        UnitId              = unitId;
        Notes               = notes?.Trim();
        Touch();
    }

    public void Activate()
    {
        Status = StudentCourseStatus.Active;
        Touch();
    }

    public void Complete(DateOnly? completionDate = null)
    {
        Status         = StudentCourseStatus.Completed;
        CompletionDate = completionDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        Touch();
    }

    public void Cancel(DateOnly? cancellationDate = null)
    {
        Status           = StudentCourseStatus.Cancelled;
        CancellationDate = cancellationDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        Touch();
    }

    public void Suspend()
    {
        Status = StudentCourseStatus.Suspended;
        Touch();
    }
}
