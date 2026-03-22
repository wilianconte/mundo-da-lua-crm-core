using HotChocolate;
using HotChocolate.Types;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Authorization;

namespace MyCRM.GraphQL.GraphQL.Students;

[QueryType]
public sealed class StudentQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Student> GetStudents(
        [Service] CRMDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Não autorizado. Faça login para continuar.")
                    .SetCode("AUTH_NOT_AUTHORIZED")
                    .Build());

        return db.Students.AsNoTracking();
    }

    public async Task<Student?> GetStudentByIdAsync(
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

        return await db.Students.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
