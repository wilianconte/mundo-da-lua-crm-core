using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Students;

[Authorize]
[QueryType]
public sealed class StudentQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Student> GetStudents([Service] CRMDbContext db) =>
        db.Students.AsNoTracking();

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Student> GetStudentById(Guid id, [Service] CRMDbContext db) =>
        db.Students.AsNoTracking().Where(x => x.Id == id);
}
