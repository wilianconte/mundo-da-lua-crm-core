using MyCRM.CRM.Application;
using MyCRM.CRM.Infrastructure;
using MyCRM.Auth.Application;
using MyCRM.Auth.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MyCRM.GraphQL.Extensions;
using MyCRM.GraphQL.Middleware;
using MyCRM.GraphQL.MultiTenancy;
using MyCRM.Shared.Kernel.MultiTenancy;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("graphql", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.QueueLimit = 0;
        opt.AutoReplenishment = true;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Multi-tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, HttpTenantService>();

// Autenticação JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthorization();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["Key"]!))
        };
    });

// Modules
builder.Services.AddCustomersApplication();
builder.Services.AddCustomersInfrastructure(builder.Configuration);
builder.Services.AddAuthApplication();
builder.Services.AddAuthInfrastructure(builder.Configuration);

// GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType()
    .AddMutationType()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Customers.CustomerQueries>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Customers.CustomerMutations>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.People.PersonQueries>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.People.PersonMutations>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Companies.CompanyQueries>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Companies.CompanyMutations>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Auth.AuthMutations>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Students.StudentQueries>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Students.StudentMutations>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.StudentGuardians.StudentGuardianQueries>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.StudentGuardians.StudentGuardianMutations>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Courses.CourseQueries>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Courses.CourseMutations>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.StudentCourses.StudentCourseQueries>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.StudentCourses.StudentCourseMutations>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Employees.EmployeeQueries>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.Employees.EmployeeMutations>()
    .AddType<MyCRM.GraphQL.GraphQL.Customers.CustomerObjectType>()
    .AddAuthorizationCore()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .ModifyCostOptions(o =>
    {
        o.MaxFieldCost = 100_000;
        o.MaxTypeCost = 100_000;
    });

var app = builder.Build();

await app.MigrateAllDbContextsAsync();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();

app.MapGraphQL().RequireRateLimiting("graphql");

if (app.Environment.IsDevelopment())
    app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();
