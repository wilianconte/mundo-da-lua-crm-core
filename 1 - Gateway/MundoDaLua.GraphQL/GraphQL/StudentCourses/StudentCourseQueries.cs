using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.StudentCourses;

[QueryType]
public sealed class StudentCourseQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<StudentCourse> GetStudentCourses(
        [Service] CRMDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Não autorizado. Faça login para continuar.")
                    .SetCode("AUTH_NOT_AUTHORIZED")
                    .Build());

        return db.StudentCourses.AsNoTracking();
    }

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<StudentCourse> GetStudentCourseById(
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

        return db.StudentCourses.AsNoTracking().Where(x => x.Id == id);
    }
}
