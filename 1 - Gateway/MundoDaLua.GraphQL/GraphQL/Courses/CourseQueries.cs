using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Courses;

[QueryType]
public sealed class CourseQueries
{
    [Authorize(Policy = SystemPermissions.CoursesRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Course> GetCourses([Service] CRMDbContext db) =>
        db.Courses.AsNoTracking();

    [Authorize(Policy = SystemPermissions.CoursesRead)]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Course> GetCourseById(Guid id, [Service] CRMDbContext db) =>
        db.Courses.AsNoTracking().Where(x => x.Id == id);
}
