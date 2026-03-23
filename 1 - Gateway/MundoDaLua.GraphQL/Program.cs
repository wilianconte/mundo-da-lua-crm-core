using MyCRM.CRM.Application;
using MyCRM.CRM.Infrastructure;
using MyCRM.Auth.Application;
using MyCRM.Auth.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MyCRM.GraphQL.Extensions;
using MyCRM.GraphQL.Middleware;
using MyCRM.GraphQL.MultiTenancy;
using MyCRM.Shared.Kernel.MultiTenancy;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
    .AddType<MyCRM.GraphQL.GraphQL.Customers.CustomerObjectType>()
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

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();

app.MapGraphQL();

if (app.Environment.IsDevelopment())
    app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();
