using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.StudentCourses;

[QueryType]
public sealed class StudentCourseQueries
{
    [Authorize(Policy = SystemPermissions.StudentCoursesRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<StudentCourse> GetStudentCourses([Service] CRMDbContext db) =>
        db.StudentCourses.AsNoTracking();

    [Authorize(Policy = SystemPermissions.StudentCoursesRead)]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<StudentCourse> GetStudentCourseById(Guid id, [Service] CRMDbContext db) =>
        db.StudentCourses.AsNoTracking().Where(x => x.Id == id);
}
