using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedFreePlanForExistingTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO auth.tenant_plans (
                    ""Id"",
                    ""TenantId"",
                    ""PlanId"",
                    ""StartDate"",
                    ""EndDate"",
                    ""IsTrial"",
                    ""FallbackPlanId"",
                    ""Status"",
                    ""PausedAt"",
                    ""CancelledAt"",
                    ""IsDeleted"",
                    ""CreatedAt""
                )
                SELECT
                    gen_random_uuid(),
                    t.""Id"",
                    'a1000000-0000-0000-0000-000000000001',
                    CURRENT_DATE,
                    NULL,
                    false,
                    NULL,
                    0,
                    NULL,
                    NULL,
                    false,
                    NOW()
                FROM auth.tenants t
                WHERE t.""IsDeleted"" = false
                  AND NOT EXISTS (
                      SELECT 1
                      FROM auth.tenant_plans tp
                      WHERE tp.""TenantId"" = t.""Id""
                        AND tp.""Status"" = 0
                  );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM auth.tenant_plans
                WHERE ""PlanId"" = 'a1000000-0000-0000-0000-000000000001'
                  AND ""IsTrial"" = false
                  AND ""FallbackPlanId"" IS NULL
                  AND ""EndDate"" IS NULL;
            ");
        }
    }
}
