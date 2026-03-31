using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MyCRM.GraphQL.GraphQL.Students.Types;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Students;

[Authorize(Policy = SystemPermissions.StudentsRead)]
[QueryType]
public sealed class StudentQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering(typeof(StudentFilterType))]
    [UseSorting(typeof(StudentSortType))]
    public IQueryable<Student> GetStudents(
        [Service] CRMDbContext db,
        StudentEnrollmentStatus? enrollmentStatus = null)
    {
        IQueryable<Student> query = db.Students.AsNoTracking();

        if (enrollmentStatus.HasValue)
        {
            if (enrollmentStatus.Value == StudentEnrollmentStatus.Active)
                query = query.Where(s => s.Courses.Any(c => c.Status == StudentCourseStatus.Active));
            else
                query = query.Where(s => !s.Courses.Any(c => c.Status == StudentCourseStatus.Active));
        }

        return query.Include(s => s.Courses);
    }

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Student> GetStudentById(Guid id, [Service] CRMDbContext db) =>
        db.Students.AsNoTracking().Include(s => s.Courses).Where(x => x.Id == id);
}
