using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// Person is the canonical master entity for all individuals in the MyCRM platform.
///
/// A single real-world person may play multiple roles simultaneously — such as
/// guardian, student, patient, employee, lead, or supplier. Rather than creating
/// duplicate records per module, Person serves as the single source of truth for
/// identity and contact data. All role-specific data lives in dedicated satellite
/// entities that reference Person by PersonId.
///
/// Planned relationships (FK defined by each role entity, not by Person):
///   Person 1──0..1  Guardian    (FK on Guardian.PersonId)
///   Person 1──0..1  Student     (FK on Student.PersonId)
///   Person 1──0..1  Patient     (FK on Patient.PersonId)
///   Person 1──0..1  Employee    (FK on Employee.PersonId)
///   Person 1──0..*  Lead        (FK on Lead.PersonId — one person, multiple campaigns)
///   Person 1──0..1  Supplier    (FK on Supplier.PersonId)
///
/// Deduplication strategy:
///   - DocumentNumber (CPF) is unique per tenant (partial index, nullable).
///   - Email is unique per tenant (partial index, nullable).
///   - Before creating a Person, check both fields via IPersonRepository.
/// </summary>
public sealed class Person : TenantEntity
{
    // ── Identity ─────────────────────────────────────────────────────────────

    /// <summary>Full legal name as registered in official documents.</summary>
    public string FullName { get; private set; } = default!;

    /// <summary>Preferred name or nickname used in day-to-day interactions.</summary>
    public string? PreferredName { get; private set; }

    /// <summary>
    /// National document number (CPF for Brazil).
    /// Primary deduplication key — unique per tenant when provided.
    /// </summary>
    public string? DocumentNumber { get; private set; }

    public DateOnly? BirthDate { get; private set; }

    public Gender? Gender { get; private set; }

    public MaritalStatus? MaritalStatus { get; private set; }

    public string? Nationality { get; private set; }

    public string? Occupation { get; private set; }

    // ── Contact ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Primary email address. Unique per tenant when provided. Stored lowercase.
    /// </summary>
    public string? Email { get; private set; }

    public string? PrimaryPhone { get; private set; }

    public string? SecondaryPhone { get; private set; }

    /// <summary>WhatsApp number — may differ from PrimaryPhone.</summary>
    public string? WhatsAppNumber { get; private set; }

    // ── Profile ───────────────────────────────────────────────────────────────

    public string? ProfileImageUrl { get; private set; }

    // ── Status & Notes ────────────────────────────────────────────────────────

    public PersonStatus Status { get; private set; }

    public string? Notes { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────

    private Person() { }

    /// <summary>
    /// Factory method — the only way to create a new Person.
    /// TenantId is also set here explicitly; the DbContext SaveChangesAsync
    /// interceptor reinforces it on INSERT.
    /// </summary>
    public static Person Create(
        Guid tenantId,
        string fullName,
        string? email = null,
        string? documentNumber = null,
        DateOnly? birthDate = null,
        string? preferredName = null,
        string? primaryPhone = null,
        string? secondaryPhone = null,
        string? whatsAppNumber = null,
        Gender? gender = null,
        MaritalStatus? maritalStatus = null,
        string? nationality = null,
        string? occupation = null,
        string? profileImageUrl = null,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        return new Person
        {
            TenantId = tenantId,
            FullName = fullName.Trim(),
            PreferredName = preferredName?.Trim(),
            DocumentNumber = documentNumber?.Trim(),
            BirthDate = birthDate,
            Email = email?.Trim().ToLowerInvariant(),
            PrimaryPhone = primaryPhone?.Trim(),
            SecondaryPhone = secondaryPhone?.Trim(),
            WhatsAppNumber = whatsAppNumber?.Trim(),
            Gender = gender,
            MaritalStatus = maritalStatus,
            Nationality = nationality?.Trim(),
            Occupation = occupation?.Trim(),
            ProfileImageUrl = profileImageUrl?.Trim(),
            Notes = notes?.Trim(),
            Status = PersonStatus.Active,
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void UpdateProfile(
        string fullName,
        string? preferredName,
        DateOnly? birthDate,
        Gender? gender,
        MaritalStatus? maritalStatus,
        string? nationality,
        string? occupation,
        string? profileImageUrl,
        string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        FullName = fullName.Trim();
        PreferredName = preferredName?.Trim();
        BirthDate = birthDate;
        Gender = gender;
        MaritalStatus = maritalStatus;
        Nationality = nationality?.Trim();
        Occupation = occupation?.Trim();
        ProfileImageUrl = profileImageUrl?.Trim();
        Notes = notes?.Trim();
        Touch();
    }

    public void UpdateContact(
        string? email,
        string? primaryPhone,
        string? secondaryPhone,
        string? whatsAppNumber)
    {
        Email = email?.Trim().ToLowerInvariant();
        PrimaryPhone = primaryPhone?.Trim();
        SecondaryPhone = secondaryPhone?.Trim();
        WhatsAppNumber = whatsAppNumber?.Trim();
        Touch();
    }

    public void UpdateDocument(string? documentNumber)
    {
        DocumentNumber = documentNumber?.Trim();
        Touch();
    }

    public void Activate() { Status = PersonStatus.Active; Touch(); }
    public void Deactivate() { Status = PersonStatus.Inactive; Touch(); }
    public void Block() { Status = PersonStatus.Blocked; Touch(); }
}
