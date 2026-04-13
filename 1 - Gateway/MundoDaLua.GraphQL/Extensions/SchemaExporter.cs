using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.GraphQL.GraphQL.Enums;
using MyCRM.GraphQL.GraphQL.Students.Types;

namespace MyCRM.GraphQL.Extensions;

/// <summary>
/// Exporta o schema GraphQL em SDL sem precisar de banco de dados ou infraestrutura.
/// Usado no CI/CD para gerar o contrato GraphQL que o front-end consome via codegen.
///
/// Invocação: dotnet run -- --export-schema [--output caminho/schema.graphql]
/// </summary>
public static class SchemaExporter
{
    public static async Task ExportAsync(string outputPath = "schema.graphql")
    {
        Console.WriteLine("Iniciando exportação do schema GraphQL...");

        var services = new ServiceCollection();

        // Autorização mínima para [Authorize(Policy = ...)] ser processado pelo HC
        services.AddAuthorization();
        services.AddLogging();

        services
            .AddGraphQLServer()
            .AddQueryType()
            .AddMutationType()
            // CRM
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Customers.CustomerQueries>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Customers.CustomerMutations>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.People.PersonQueries>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.People.PersonMutations>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Companies.CompanyQueries>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Companies.CompanyMutations>()
            // Auth
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Auth.AuthMutations>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Auth.UserQueries>()
            .AddType<MyCRM.GraphQL.GraphQL.Auth.UserObjectType>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Auth.RoleQueries>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Auth.RoleMutations>()
            .AddType<MyCRM.GraphQL.GraphQL.Auth.RoleObjectType>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Auth.PermissionQueries>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Auth.PermissionAdminQueries>()
            // Students
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Students.StudentQueries>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Students.StudentMutations>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Students.StudentObjectTypeExtension>()
            .AddType<StudentEnrollmentStatusType>()
            .AddType<StudentCourseStatusType>()
            .AddType<StudentFilterType>()
            .AddType<StudentSortType>()
            // Student Guardians
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.StudentGuardians.StudentGuardianQueries>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.StudentGuardians.StudentGuardianMutations>()
            // Courses
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Courses.CourseQueries>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Courses.CourseMutations>()
            // Student Courses
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.StudentCourses.StudentCourseQueries>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.StudentCourses.StudentCourseMutations>()
            // Employees
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Employees.EmployeeQueries>()
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Employees.EmployeeMutations>()
            // Tenants
            .AddTypeExtension<MyCRM.GraphQL.GraphQL.Tenants.TenantMutations>()
            // Shared types
            .AddType<MyCRM.GraphQL.GraphQL.Customers.CustomerObjectType>()
            // Capabilities
            .AddAuthorization()
            .AddFiltering()
            .AddSorting()
            .AddProjections();

        await using var provider = services.BuildServiceProvider();

        var resolver = provider.GetRequiredService<IRequestExecutorResolver>();
        var executor = await resolver.GetRequestExecutorAsync();

        var sdl = executor.Schema.ToString();

        var directory = System.IO.Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            System.IO.Directory.CreateDirectory(directory);

        await System.IO.File.WriteAllTextAsync(outputPath, sdl);

        Console.WriteLine($"Schema exportado: {System.IO.Path.GetFullPath(outputPath)}");
    }
}
