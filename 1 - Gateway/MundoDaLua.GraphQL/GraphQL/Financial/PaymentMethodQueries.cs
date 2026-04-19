using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Financial;

[QueryType]
public sealed class PaymentMethodQueries
{
    [Authorize(Policy = SystemPermissions.PaymentMethodsRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PaymentMethod> GetPaymentMethods([Service] CRMDbContext db) =>
        db.PaymentMethods.AsNoTracking();
}
