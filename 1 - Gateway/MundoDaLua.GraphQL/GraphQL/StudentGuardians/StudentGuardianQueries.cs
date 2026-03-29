using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.StudentGuardians;

[Authorize]
[QueryType]
public sealed class StudentGuardianQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<StudentGuardian> GetStudentGuardians([Service] CRMDbContext db) =>
        db.StudentGuardians.AsNoTracking();

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<StudentGuardian> GetStudentGuardianById(Guid id, [Service] CRMDbContext db) =>
        db.StudentGuardians.AsNoTracking().Where(x => x.Id == id);
}
