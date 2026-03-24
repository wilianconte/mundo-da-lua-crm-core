using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// Employee is a role-specific extension of Person for the employment context.
///
/// This entity does NOT duplicate personal data (name, phone, email, document).
/// All identity data remains in the referenced Person entity.
/// Employee stores only employment-specific data such as hire date, position,
/// department, contract type, and payroll information.
///
/// Relationship:
///   Person 1──0..1 Employee  (FK on Employee.PersonId)
///
/// A Person should not have duplicate active Employee records within the same tenant.
/// A Person may hold other roles simultaneously (Student, Guardian, Patient, etc.).
/// </summary>
public sealed class Employee : TenantEntity
{
    // ── Person Reference ─────────────────────────────────────────────────────

    /// <summary>Reference to the master Person entity. Never store personal data here.</summary>
    public Guid PersonId { get; private set; }

    /// <summary>Navigation property for EF Core — do not use directly in domain logic.</summary>
    public Person? Person { get; private set; }

    // ── Employment ────────────────────────────────────────────────────────────

    /// <summary>Internal employee code / badge number (unique per tenant when set).</summary>
    public string? EmployeeCode { get; private set; }

    /// <summary>Date the employee was officially hired.</summary>
    public DateOnly? HireDate { get; private set; }

    /// <summary>Date the employment ended (null if still active).</summary>
    public DateOnly? TerminationDate { get; private set; }

    /// <summary>Job title or position (e.g. "Teacher", "Psychologist", "Admin").</summary>
    public string? Position { get; private set; }

    /// <summary>Department or area within the organization (e.g. "Pedagogy", "HR", "Clinic").</summary>
    public string? Department { get; private set; }

    // ── Contract & Schedule ───────────────────────────────────────────────────

    /// <summary>Type of employment contract (e.g. "CLT", "PJ", "Freelancer", "Volunteer").</summary>
    public string? ContractType { get; private set; }

    /// <summary>Work schedule description (e.g. "Mon–Fri 08:00–17:00", "Shifts").</summary>
    public string? WorkSchedule { get; private set; }

    /// <summary>Weekly or monthly workload in hours.</summary>
    public decimal? WorkloadHours { get; private set; }

    // ── Payroll & Org ─────────────────────────────────────────────────────────

    /// <summary>Payroll or HR system identifier.</summary>
    public string? PayrollNumber { get; private set; }

    /// <summary>Reference to the manager's Employee record (self-referencing, optional).</summary>
    public Guid? ManagerEmployeeId { get; private set; }

    /// <summary>Navigation property for the manager — do not use directly in domain logic.</summary>
    public Employee? Manager { get; private set; }

    /// <summary>Reference to the unit/branch where the employee is assigned.</summary>
    public Guid? UnitId { get; private set; }

    /// <summary>Cost center code for financial allocation.</summary>
    public string? CostCenter { get; private set; }

    // ── Status & Notes ────────────────────────────────────────────────────────

    public EmployeeStatus Status { get; private set; }

    /// <summary>Convenience flag — true when Status is Active. Managed by domain methods.</summary>
    public bool IsActive { get; private set; }

    public string? Notes { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────

    private Employee() { }

    /// <summary>
    /// Factory method — the only way to create a new Employee.
    /// Requires a valid PersonId. Personal data is NOT stored here.
    /// </summary>
    public static Employee Create(
        Guid tenantId,
        Guid personId,
        string? employeeCode       = null,
        DateOnly? hireDate         = null,
        string? position           = null,
        string? department         = null,
        string? contractType       = null,
        string? workSchedule       = null,
        decimal? workloadHours     = null,
        string? payrollNumber      = null,
        Guid? managerEmployeeId    = null,
        Guid? unitId               = null,
        string? costCenter         = null,
        string? notes              = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("PersonId is required.", nameof(personId));

        return new Employee
        {
            TenantId          = tenantId,
            PersonId          = personId,
            EmployeeCode      = employeeCode?.Trim(),
            HireDate          = hireDate,
            Position          = position?.Trim(),
            Department        = department?.Trim(),
            ContractType      = contractType?.Trim(),
            WorkSchedule      = workSchedule?.Trim(),
            WorkloadHours     = workloadHours,
            PayrollNumber     = payrollNumber?.Trim(),
            ManagerEmployeeId = managerEmployeeId,
            UnitId            = unitId,
            CostCenter        = costCenter?.Trim(),
            Status            = EmployeeStatus.Active,
            IsActive          = true,
            Notes             = notes?.Trim(),
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void UpdateInfo(
        string? employeeCode,
        DateOnly? hireDate,
        string? position,
        string? department,
        string? contractType,
        string? workSchedule,
        decimal? workloadHours,
        string? payrollNumber,
        Guid? managerEmployeeId,
        Guid? unitId,
        string? costCenter,
        string? notes)
    {
        EmployeeCode      = employeeCode?.Trim();
        HireDate          = hireDate;
        Position          = position?.Trim();
        Department        = department?.Trim();
        ContractType      = contractType?.Trim();
        WorkSchedule      = workSchedule?.Trim();
        WorkloadHours     = workloadHours;
        PayrollNumber     = payrollNumber?.Trim();
        ManagerEmployeeId = managerEmployeeId;
        UnitId            = unitId;
        CostCenter        = costCenter?.Trim();
        Notes             = notes?.Trim();
        Touch();
    }

    public void Terminate(DateOnly? terminationDate = null)
    {
        TerminationDate = terminationDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        Status          = EmployeeStatus.Terminated;
        IsActive        = false;
        Touch();
    }

    public void Activate()   { Status = EmployeeStatus.Active;     IsActive = true;  Touch(); }
    public void Deactivate() { Status = EmployeeStatus.Inactive;   IsActive = false; Touch(); }
    public void PutOnLeave() { Status = EmployeeStatus.OnLeave;    IsActive = false; Touch(); }
    public void Suspend()    { Status = EmployeeStatus.Suspended;  IsActive = false; Touch(); }
}
