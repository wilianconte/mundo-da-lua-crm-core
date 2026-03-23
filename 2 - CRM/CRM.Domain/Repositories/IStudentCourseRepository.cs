using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IStudentCourseRepository : IRepository<StudentCourse>
{
    /// <summary>
    /// Returns true if the student already has an active or pending enrollment
    /// in the given course within the tenant. Used to prevent simultaneous
    /// duplicate active enrollments for the same student+course combination.
    /// </summary>
    Task<bool> ActiveEnrollmentExistsAsync(Guid tenantId, Guid studentId, Guid courseId, Guid? excludeId = null, CancellationToken ct = default);

    Task<IReadOnlyList<StudentCourse>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default);
    Task<IReadOnlyList<StudentCourse>> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default);
}
