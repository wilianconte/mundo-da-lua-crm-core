namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// Represents the type/category of an educational offering or program.
/// Kept generic to support different business contexts (school, language school, workshop center, etc.).
/// </summary>
public enum CourseType
{
    /// <summary>After-school reinforcement or tutoring program.</summary>
    AfterSchool = 1,

    /// <summary>Language course (e.g. English, Spanish, Portuguese).</summary>
    Language = 2,

    /// <summary>School class or grade group (turma escolar).</summary>
    SchoolClass = 3,

    /// <summary>Short-term workshop or event-based program.</summary>
    Workshop = 4,

    /// <summary>Any other structured educational or service offering not covered above.</summary>
    Other = 5,
}
