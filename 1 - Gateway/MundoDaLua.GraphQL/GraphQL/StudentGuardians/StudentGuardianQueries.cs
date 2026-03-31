using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.StudentGuardians;

[QueryType]
public sealed class StudentGuardianQueries
{
    [Authorize(Policy = SystemPermissions.StudentGuardiansRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<StudentGuardian> GetStudentGuardians([Service] CRMDbContext db) =>
        db.StudentGuardians.AsNoTracking();

    [Authorize(Policy = SystemPermissions.StudentGuardiansRead)]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<StudentGuardian> GetStudentGuardianById(Guid id, [Service] CRMDbContext db) =>
        db.StudentGuardians.AsNoTracking().Where(x => x.Id == id);
}
