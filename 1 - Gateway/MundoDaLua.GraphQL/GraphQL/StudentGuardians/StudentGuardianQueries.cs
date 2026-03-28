using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.GraphQL.GraphQL.StudentGuardians;

[QueryType]
public sealed class StudentGuardianQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<StudentGuardian> GetStudentGuardians(
        [Service] CRMDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Não autorizado. Faça login para continuar.")
                    .SetCode("AUTH_NOT_AUTHORIZED")
                    .Build());

        return db.StudentGuardians.AsNoTracking();
    }

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<StudentGuardian> GetStudentGuardianById(
        Guid id,
        [Service] CRMDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Não autorizado. Faça login para continuar.")
                    .SetCode("AUTH_NOT_AUTHORIZED")
                    .Build());

        return db.StudentGuardians.AsNoTracking().Where(x => x.Id == id);
    }
}
