using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MyCRM.GraphQL.GraphQL.Students.Types;
using MyCRM.CRM.Application.DTOs;

namespace MyCRM.GraphQL.GraphQL.Students;

[Authorize]
[QueryType]
public sealed class StudentQueries
{
    /// <summary>
    /// Get all students with pagination, filtering and sorting.
    /// Supports filtering by enrollmentStatus (ACTIVE/INACTIVE).
    /// </summary>
    /// <param name="db">The database context</param>
    /// <param name="enrollmentStatus">Filter by enrollment status: ACTIVE (has active course) or INACTIVE (no active course)</param>
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering(typeof(StudentFilterType))]
    [UseSorting(typeof(StudentSortType))]
    public IQueryable<Student> GetStudents(
        [Service] CRMDbContext db,
        StudentEnrollmentStatus? enrollmentStatus = null)
    {
        IQueryable<Student> query = db.Students.AsNoTracking();

        // Apply enrollmentStatus filter if provided
        // ACTIVE: has at least one StudentCourse with Status = Active
        // INACTIVE: no active enrollment (Pending does NOT count as active)
        if (enrollmentStatus.HasValue)
        {
            if (enrollmentStatus.Value == StudentEnrollmentStatus.Active)
            {
                query = query.Where(s => s.Courses.Any(c => c.Status == StudentCourseStatus.Active));
            }
            else
            {
                query = query.Where(s => !s.Courses.Any(c => c.Status == StudentCourseStatus.Active));
            }
        }

        // Always include Courses for the computed enrollmentStatus field
        return query.Include(s => s.Courses);
    }

    /// <summary>
    /// Get a single student by ID.
    /// </summary>
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Student> GetStudentById(Guid id, [Service] CRMDbContext db) =>
        db.Students.AsNoTracking().Include(s => s.Courses).Where(x => x.Id == id);
}
