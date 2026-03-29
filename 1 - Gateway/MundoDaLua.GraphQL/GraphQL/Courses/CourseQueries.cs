using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Courses;

[Authorize]
[QueryType]
public sealed class CourseQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Course> GetCourses([Service] CRMDbContext db) =>
        db.Courses.AsNoTracking();

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Course> GetCourseById(Guid id, [Service] CRMDbContext db) =>
        db.Courses.AsNoTracking().Where(x => x.Id == id);
}
