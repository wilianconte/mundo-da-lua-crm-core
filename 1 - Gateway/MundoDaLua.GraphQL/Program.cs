using MyCRM.CRM.Application;
using MyCRM.CRM.Infrastructure;
using MyCRM.Auth.Application;
using MyCRM.Auth.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MyCRM.GraphQL.Authorization;
using MyCRM.GraphQL.Extensions;
using MyCRM.GraphQL.Middleware;
using MyCRM.GraphQL.MultiTenancy;
using MyCRM.GraphQL.Services;
using MyCRM.Shared.Kernel;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.MultiTenancy;
using HotChocolate.Execution;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

// Modo de exportação de schema — sem DB, sem JWT, sem infraestrutura.
// Usado no CI/CD para gerar contracts/schema.graphql.
if (args.Contains("--export-schema"))
{
    var outputPath = args.SkipWhile(a => a != "--output").Skip(1).FirstOrDefault()
        ?? "schema.graphql";
    await MyCRM.GraphQL.Extensions.SchemaExporter.ExportAsync(outputPath);
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext());

// CORS
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

if (allowedOrigins.Length == 0 && !builder.Environment.IsDevelopment())
    throw new InvalidOperationException(
        "CORS não configurado. Defina Cors:AllowedOrigins (ou CORS__AllowedOrigins__0) antes de iniciar em produção.");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

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
builder.Services.AddScoped<ICurrentUserService, HttpCurrentUserService>();

// Autenticação JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key não configurado ou muito curto. " +
        "Defina a variável de ambiente Jwt__Key com no mínimo 32 caracteres.");

builder.Services.AddAuthorization(opts =>
{
    foreach (var (name, _) in SystemPermissions.All)
        opts.AddPolicy(name, b => b.Requirements.Add(new PermissionRequirement(name)));
});

builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionAuthorizationHandler>();

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
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Exception handler
builder.Services.AddExceptionHandler<MyCRM.GraphQL.Middleware.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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
    .AddCrmGraphQL()
    .AddAuthGraphQL()
    .AddAuthorization()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .ModifyRequestOptions(opt =>
    {
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    })
    .DisableIntrospection(!builder.Environment.IsDevelopment())
    .ModifyCostOptions(o =>
    {
        o.MaxFieldCost = 100_000;
        o.MaxTypeCost = 100_000;
    });

var app = builder.Build();

if (args.Contains("--migrate"))
{
    await app.MigrateAllDbContextsAsync();
    return;
}

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();

app.MapGraphQL().RequireRateLimiting("graphql");

// Expõe o schema SDL em /contracts/schema.graphql para consumo pelo front-end (codegen).
// Em Development: livre.
// Em outros ambientes: exige Authorization: Bearer <SchemaExport:Token>.
// Se SchemaExport:Token não estiver configurado fora de Development, o endpoint retorna 404.
var schemaExportToken = app.Configuration["SchemaExport:Token"];

app.MapGet("/contracts/schema.graphql", async (HttpContext ctx, IRequestExecutorResolver resolver) =>
{
    if (!app.Environment.IsDevelopment())
    {
        if (string.IsNullOrWhiteSpace(schemaExportToken))
            return Results.NotFound();

        var auth = ctx.Request.Headers.Authorization.ToString();
        if (auth != $"Bearer {schemaExportToken}")
            return Results.Unauthorized();
    }

    var executor = await resolver.GetRequestExecutorAsync();
    return Results.Content(executor.Schema.ToString(), "text/plain; charset=utf-8");
});

if (app.Environment.IsDevelopment())
    app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();
