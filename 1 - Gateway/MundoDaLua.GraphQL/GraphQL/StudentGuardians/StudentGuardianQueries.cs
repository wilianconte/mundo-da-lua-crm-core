using HotChocolate;
using HotChocolate.Types;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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

    public async Task<StudentGuardian?> GetStudentGuardianByIdAsync(
        Guid id,
        [Service] CRMDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor,
        CancellationToken ct)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Não autorizado. Faça login para continuar.")
                    .SetCode("AUTH_NOT_AUTHORIZED")
                    .Build());

        return await db.StudentGuardians.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
