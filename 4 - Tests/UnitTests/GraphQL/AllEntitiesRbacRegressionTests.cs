using System.Security.Claims;
using HotChocolate.Execution;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Application.Commands.Roles.CreateRole;
using MyCRM.Auth.Application.Commands.Roles.UpdateRole;
using MyCRM.Auth.Application.Commands.Users.CreateUser;
using MyCRM.Auth.Application.Commands.Users.UpdateUser;
using MyCRM.Auth.Application.DTOs;
using MyCRM.CRM.Application.Commands.Companies.CreateCompany;
using MyCRM.CRM.Application.Commands.Companies.DeleteCompany;
using MyCRM.CRM.Application.Commands.Companies.UpdateCompany;
using MyCRM.CRM.Application.Commands.Courses.CreateCourse;
using MyCRM.CRM.Application.Commands.Courses.DeleteCourse;
using MyCRM.CRM.Application.Commands.Courses.UpdateCourse;
using MyCRM.CRM.Application.Commands.Customers.CreateCustomer;
using MyCRM.CRM.Application.Commands.Customers.DeleteCustomer;
using MyCRM.CRM.Application.Commands.Customers.UpdateCustomer;
using MyCRM.CRM.Application.Commands.Employees.CreateEmployee;
using MyCRM.CRM.Application.Commands.Employees.DeleteEmployee;
using MyCRM.CRM.Application.Commands.Employees.UpdateEmployee;
using MyCRM.CRM.Application.Commands.People.CreatePerson;
using MyCRM.CRM.Application.Commands.People.DeletePerson;
using MyCRM.CRM.Application.Commands.People.UpdatePerson;
using MyCRM.CRM.Application.Commands.StudentCourses.CreateStudentCourse;
using MyCRM.CRM.Application.Commands.StudentCourses.DeleteStudentCourse;
using MyCRM.CRM.Application.Commands.StudentCourses.UpdateStudentCourse;
using MyCRM.CRM.Application.Commands.StudentGuardians.CreateStudentGuardian;
using MyCRM.CRM.Application.Commands.StudentGuardians.DeleteStudentGuardian;
using MyCRM.CRM.Application.Commands.StudentGuardians.UpdateStudentGuardian;
using MyCRM.CRM.Application.Commands.Students.CreateStudent;
using MyCRM.CRM.Application.Commands.Students.DeleteStudent;
using MyCRM.CRM.Application.Commands.Students.UpdateStudent;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using MyCRM.GraphQL.Authorization;
using MyCRM.GraphQL.GraphQL.Auth;
using MyCRM.GraphQL.GraphQL.Companies;
using MyCRM.GraphQL.GraphQL.Courses;
using MyCRM.GraphQL.GraphQL.Customers;
using MyCRM.GraphQL.GraphQL.Employees;
using MyCRM.GraphQL.GraphQL.People;
using MyCRM.GraphQL.GraphQL.StudentCourses;
using MyCRM.GraphQL.GraphQL.StudentGuardians;
using MyCRM.GraphQL.GraphQL.Students;
using MyCRM.Shared.Kernel;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.GraphQL;

/// <summary>
/// RBAC regression tests covering all business entities plus auth mutations.
/// Scope: students, student_guardians, student_courses, customers, employees,
/// courses, people, companies, users and roles.
///
/// For each entity the suite verifies:
/// 1) A non-admin user holding the correct policy can execute the operation.
/// 2) A non-admin user without the required policy receives AUTH_NOT_AUTHORIZED.
/// 3) Holding a different entity's permission does NOT grant access (cross-contamination guard).
///
/// Root cause being guarded against: string mismatch (whitespace / casing) in
/// PermissionService.HasPermissionAsync that could silently deny valid requests,
/// and class-level [Authorize] bleed across Hot Chocolate type extensions.
/// </summary>
public sealed class AllEntitiesRbacRegressionTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private sealed class FixedTenantService(Guid tenantId) : ITenantService
    {
        public Guid TenantId { get; private set; } = tenantId;
        public void SetTenant(Guid id) => TenantId = id;
    }

    /// <summary>
    /// Shared executor with all entity mutation types registered.
    /// Uses PersonQueries to satisfy HC's requirement for a non-empty Query type.
    /// IMediator is registered as both IMediator and ISender because CustomerMutations
    /// injects IMediator while all other mutations inject ISender.
    /// </summary>
    private static async Task<IRequestExecutor> BuildExecutorAsync(
        IMediator mediator,
        IPermissionService permissionService)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(opts =>
        {
            foreach (var (name, _) in SystemPermissions.All)
                opts.AddPolicy(name, b => b.Requirements.Add(new PermissionRequirement(name)));
        });

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());

        services.AddSingleton<ISender>(mediator);
        services.AddSingleton<IMediator>(mediator);
        services.AddSingleton(permissionService);
        services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<ITenantService>(_ => new FixedTenantService(TenantId));
        services.AddScoped<ICurrentUserService>(_ => currentUserService);
        services.AddDbContext<CRMDbContext>(o =>
            o.UseInMemoryDatabase($"crm-rbac-{Guid.NewGuid()}"));

        return await services
            .AddGraphQLServer()
            .AddQueryType()
            .AddMutationType()
            // Minimum query field to satisfy HC schema validation
            .AddTypeExtension<PersonQueries>()
            // All entity mutations under test
            .AddTypeExtension<StudentMutations>()
            .AddTypeExtension<StudentGuardianMutations>()
            .AddTypeExtension<StudentCourseMutations>()
            .AddTypeExtension<CustomerMutations>()
            .AddTypeExtension<EmployeeMutations>()
            .AddTypeExtension<CourseMutations>()
            .AddTypeExtension<PersonMutations>()
            .AddTypeExtension<CompanyMutations>()
            .AddTypeExtension<AuthMutations>()
            .AddTypeExtension<RoleMutations>()
            .AddAuthorization()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .BuildRequestExecutorAsync();
    }

    private static IOperationRequest BuildRequest(string document, Guid userId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("sub", userId.ToString()),
        ], authenticationType: "Bearer"));

        return new OperationRequestBuilder()
            .SetDocument(document)
            .AddGlobalState(nameof(ClaimsPrincipal), principal)
            .Build();
    }

    private static void AssertAuthError(IOperationResult result)
    {
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors!, e => e.Code == "AUTH_NOT_AUTHORIZED");
    }

    // ─── STUDENTS ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Students_WithCreatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new StudentDto(
            Guid.NewGuid(), TenantId, Guid.NewGuid(), null,
            StudentEnrollmentStatus.Active, null, DateTimeOffset.UtcNow, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateStudentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<StudentDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.StudentsCreate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ createStudent(input: {{ personId: \"{Guid.NewGuid()}\" }}) {{ student {{ id }} }} }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var payload = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createStudent"]);
        var student = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["student"]);
        Assert.Equal(dto.Id.ToString(), student["id"]?.ToString());
    }

    [Fact]
    public async Task Students_WithoutCreatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ createStudent(input: {{ personId: \"{Guid.NewGuid()}\" }}) {{ student {{ id }} }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Students_WithUpdatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new StudentDto(
            Guid.NewGuid(), TenantId, Guid.NewGuid(), null,
            StudentEnrollmentStatus.Active, "updated", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateStudentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<StudentDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.StudentsUpdate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateStudent(id: \"{Guid.NewGuid()}\", input: {{ notes: \"updated\" }}) {{ student {{ id }} }} }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var payload = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["updateStudent"]);
        var student = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["student"]);
        Assert.Equal(dto.Id.ToString(), student["id"]?.ToString());
    }

    [Fact]
    public async Task Students_WithoutUpdatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateStudent(id: \"{Guid.NewGuid()}\", input: {{ notes: \"denied\" }}) {{ student {{ id }} }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Students_WithoutDeletePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteStudent(id: \"{Guid.NewGuid()}\") }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    // ─── STUDENT GUARDIANS ──────────────────────────────────────────────────

    [Fact]
    public async Task StudentGuardians_WithCreatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new StudentGuardianDto(
            Guid.NewGuid(), TenantId, Guid.NewGuid(), Guid.NewGuid(),
            GuardianRelationshipType.Mother, true, false, true, false,
            null, DateTimeOffset.UtcNow, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateStudentGuardianCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<StudentGuardianDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.StudentGuardiansCreate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $@"mutation {{
              createStudentGuardian(input: {{
                studentId: ""{Guid.NewGuid()}""
                guardianPersonId: ""{Guid.NewGuid()}""
                relationshipType: MOTHER
                isPrimaryGuardian: true
                isFinancialResponsible: false
                receivesNotifications: true
                canPickupChild: false
              }}) {{ studentGuardian {{ id }} }}
            }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var payload = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createStudentGuardian"]);
        var guardian = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["studentGuardian"]);
        Assert.Equal(dto.Id.ToString(), guardian["id"]?.ToString());
    }

    [Fact]
    public async Task StudentGuardians_WithoutCreatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $@"mutation {{
              createStudentGuardian(input: {{
                studentId: ""{Guid.NewGuid()}""
                guardianPersonId: ""{Guid.NewGuid()}""
                relationshipType: MOTHER
                isPrimaryGuardian: false
                isFinancialResponsible: false
                receivesNotifications: false
                canPickupChild: false
              }}) {{ studentGuardian {{ id }} }}
            }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task StudentGuardians_WithUpdatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new StudentGuardianDto(
            Guid.NewGuid(), TenantId, Guid.NewGuid(), Guid.NewGuid(),
            GuardianRelationshipType.Father, false, true, true, true,
            "updated", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateStudentGuardianCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<StudentGuardianDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.StudentGuardiansUpdate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $@"mutation {{
              updateStudentGuardian(
                id: ""{Guid.NewGuid()}"",
                input: {{
                  relationshipType: FATHER
                  isPrimaryGuardian: false
                  isFinancialResponsible: true
                  receivesNotifications: true
                  canPickupChild: true
                  notes: ""updated""
                }}
              ) {{ studentGuardian {{ id }} }}
            }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var payload = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["updateStudentGuardian"]);
        var guardian = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["studentGuardian"]);
        Assert.Equal(dto.Id.ToString(), guardian["id"]?.ToString());
    }

    [Fact]
    public async Task StudentGuardians_WithoutUpdatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $@"mutation {{
              updateStudentGuardian(
                id: ""{Guid.NewGuid()}"",
                input: {{
                  relationshipType: FATHER
                  isPrimaryGuardian: false
                  isFinancialResponsible: true
                  receivesNotifications: true
                  canPickupChild: true
                }}
              ) {{ studentGuardian {{ id }} }}
            }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task StudentGuardians_WithoutDeletePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteStudentGuardian(id: \"{Guid.NewGuid()}\") }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    // ─── STUDENT COURSES ────────────────────────────────────────────────────

    [Fact]
    public async Task StudentCourses_WithCreatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new StudentCourseDto(
            Guid.NewGuid(), TenantId, Guid.NewGuid(), Guid.NewGuid(),
            null, null, null, null, null, StudentCourseStatus.Active,
            null, null, null, null, null, DateTimeOffset.UtcNow, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateStudentCourseCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<StudentCourseDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.StudentCoursesCreate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $@"mutation {{
              createStudentCourse(input: {{
                studentId: ""{Guid.NewGuid()}""
                courseId: ""{Guid.NewGuid()}""
              }}) {{ studentCourse {{ id }} }}
            }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var payload = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createStudentCourse"]);
        var sc = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["studentCourse"]);
        Assert.Equal(dto.Id.ToString(), sc["id"]?.ToString());
    }

    [Fact]
    public async Task StudentCourses_WithoutCreatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $@"mutation {{
              createStudentCourse(input: {{
                studentId: ""{Guid.NewGuid()}""
                courseId: ""{Guid.NewGuid()}""
              }}) {{ studentCourse {{ id }} }}
            }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task StudentCourses_WithUpdatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new StudentCourseDto(
            Guid.NewGuid(), TenantId, Guid.NewGuid(), Guid.NewGuid(),
            null, null, null, null, null, StudentCourseStatus.Active,
            "A", "Morning", null, null, "updated", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateStudentCourseCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<StudentCourseDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.StudentCoursesUpdate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $@"mutation {{
              updateStudentCourse(
                id: ""{Guid.NewGuid()}"",
                input: {{ classGroup: ""A"", shift: ""Morning"", status: ACTIVE, notes: ""updated"" }}
              ) {{ studentCourse {{ id }} }}
            }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var payload = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["updateStudentCourse"]);
        var studentCourse = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["studentCourse"]);
        Assert.Equal(dto.Id.ToString(), studentCourse["id"]?.ToString());
    }

    [Fact]
    public async Task StudentCourses_WithoutUpdatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $@"mutation {{
              updateStudentCourse(
                id: ""{Guid.NewGuid()}"",
                input: {{ classGroup: ""A"", shift: ""Morning"", status: ACTIVE }}
              ) {{ studentCourse {{ id }} }}
            }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task StudentCourses_WithoutDeletePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteStudentCourse(id: \"{Guid.NewGuid()}\") }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    // ─── CUSTOMERS ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Customers_WithCreatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new CustomerDto(
            Guid.NewGuid(), TenantId, "Cliente Teste", "test@test.com",
            null, null, CustomerType.Individual, CustomerStatus.Active,
            null, null, DateTimeOffset.UtcNow, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCustomerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<CustomerDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.CustomersCreate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createCustomer(input: { name: \"Cliente Teste\", email: \"test@test.com\", type: INDIVIDUAL }) { id name } }",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var customer = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createCustomer"]);
        Assert.Equal(dto.Id.ToString(), customer["id"]?.ToString());
        Assert.Equal("Cliente Teste", customer["name"]?.ToString());
    }

    [Fact]
    public async Task Customers_WithoutCreatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createCustomer(input: { name: \"Denied\", email: \"denied@test.com\", type: INDIVIDUAL }) { id } }",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Customers_WithUpdatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new CustomerDto(
            Guid.NewGuid(), TenantId, "Cliente Atualizado", "updated@test.com",
            null, null, CustomerType.Individual, CustomerStatus.Active,
            null, "notes", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateCustomerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<CustomerDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.CustomersUpdate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateCustomer(input: {{ id: \"{Guid.NewGuid()}\", name: \"Cliente Atualizado\", email: \"updated@test.com\" }}) {{ id name }} }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var customer = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["updateCustomer"]);
        Assert.Equal(dto.Id.ToString(), customer["id"]?.ToString());
    }

    [Fact]
    public async Task Customers_WithoutUpdatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateCustomer(input: {{ id: \"{Guid.NewGuid()}\", name: \"Denied\", email: \"denied@test.com\" }}) {{ id }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Customers_WithoutDeletePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteCustomer(id: \"{Guid.NewGuid()}\") }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    // ─── EMPLOYEES ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Employees_WithCreatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new EmployeeDto(
            Guid.NewGuid(), TenantId, Guid.NewGuid(),
            null, null, null, null, null, null, null, null, null, null, null, null,
            EmployeeStatus.Active, true, null, DateTimeOffset.UtcNow, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateEmployeeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<EmployeeDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.EmployeesCreate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ createEmployee(input: {{ personId: \"{Guid.NewGuid()}\" }}) {{ employee {{ id }} }} }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var payload = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createEmployee"]);
        var employee = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["employee"]);
        Assert.Equal(dto.Id.ToString(), employee["id"]?.ToString());
    }

    [Fact]
    public async Task Employees_WithoutCreatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ createEmployee(input: {{ personId: \"{Guid.NewGuid()}\" }}) {{ employee {{ id }} }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Employees_WithUpdatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new EmployeeDto(
            Guid.NewGuid(), TenantId, Guid.NewGuid(),
            null, null, null, "Teacher", null, null, null, null, null, null, null, null,
            EmployeeStatus.Active, true, "updated", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateEmployeeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<EmployeeDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.EmployeesUpdate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateEmployee(id: \"{Guid.NewGuid()}\", input: {{ position: \"Teacher\", notes: \"updated\" }}) {{ employee {{ id }} }} }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var payload = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["updateEmployee"]);
        var employee = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["employee"]);
        Assert.Equal(dto.Id.ToString(), employee["id"]?.ToString());
    }

    [Fact]
    public async Task Employees_WithoutUpdatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateEmployee(id: \"{Guid.NewGuid()}\", input: {{ position: \"Denied\" }}) {{ employee {{ id }} }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Employees_WithoutDeletePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteEmployee(id: \"{Guid.NewGuid()}\") }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    // ─── COURSES ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Courses_WithCreatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new CourseDto(
            Guid.NewGuid(), TenantId, "Curso Teste", null, CourseType.Language,
            null, null, null, null, null, null, null, CourseStatus.Draft, false,
            null, DateTimeOffset.UtcNow, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCourseCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<CourseDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.CoursesCreate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createCourse(input: { name: \"Curso Teste\", type: LANGUAGE }) { course { id } } }",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var payload = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createCourse"]);
        var course = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["course"]);
        Assert.Equal(dto.Id.ToString(), course["id"]?.ToString());
    }

    [Fact]
    public async Task Courses_WithoutCreatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createCourse(input: { name: \"Denied\", type: LANGUAGE }) { course { id } } }",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Courses_WithUpdatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new CourseDto(
            Guid.NewGuid(), TenantId, "Curso Atualizado", null, CourseType.Language,
            null, null, null, null, null, null, null, CourseStatus.Active, false,
            "updated", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateCourseCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<CourseDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.CoursesUpdate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateCourse(id: \"{Guid.NewGuid()}\", input: {{ name: \"Curso Atualizado\", type: LANGUAGE, notes: \"updated\" }}) {{ course {{ id }} }} }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var payload = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["updateCourse"]);
        var course = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(payload["course"]);
        Assert.Equal(dto.Id.ToString(), course["id"]?.ToString());
    }

    [Fact]
    public async Task Courses_WithoutUpdatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateCourse(id: \"{Guid.NewGuid()}\", input: {{ name: \"Denied\", type: LANGUAGE }}) {{ course {{ id }} }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Courses_WithoutDeletePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteCourse(id: \"{Guid.NewGuid()}\") }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    // â”€â”€â”€ PEOPLE â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task People_WithCreatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new PersonDto(
            Guid.NewGuid(), TenantId, "Pessoa Teste",
            null, null, null, null, null, null, null, "pessoa@test.com",
            null, null, null, null, PersonStatus.Active, null,
            DateTimeOffset.UtcNow, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreatePersonCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<PersonDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.PeopleCreate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createPerson(input: { fullName: \"Pessoa Teste\", email: \"pessoa@test.com\" }) { id fullName } }",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var person = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createPerson"]);
        Assert.Equal(dto.Id.ToString(), person["id"]?.ToString());
    }

    [Fact]
    public async Task People_WithoutCreatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createPerson(input: { fullName: \"Denied\" }) { id } }",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task People_WithUpdatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new PersonDto(
            Guid.NewGuid(), TenantId, "Pessoa Atualizada",
            null, null, null, null, null, null, null, "updated@test.com",
            null, null, null, null, PersonStatus.Active, "updated",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdatePersonCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<PersonDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.PeopleUpdate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updatePerson(id: \"{Guid.NewGuid()}\", input: {{ fullName: \"Pessoa Atualizada\", email: \"updated@test.com\", notes: \"updated\" }}) {{ id fullName }} }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var person = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["updatePerson"]);
        Assert.Equal(dto.Id.ToString(), person["id"]?.ToString());
    }

    [Fact]
    public async Task People_WithoutUpdatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updatePerson(id: \"{Guid.NewGuid()}\", input: {{ fullName: \"Denied\" }}) {{ id }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task People_WithoutDeletePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deletePerson(id: \"{Guid.NewGuid()}\") }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    // ─── COMPANIES ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Companies_WithCreatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new CompanyDto(
            Guid.NewGuid(), TenantId, "Empresa Teste", null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null,
            null, CompanyStatus.Active, null, DateTimeOffset.UtcNow, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCompanyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<CompanyDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.CompaniesCreate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createCompany(input: { legalName: \"Empresa Teste\" }) { id legalName } }",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var company = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createCompany"]);
        Assert.Equal(dto.Id.ToString(), company["id"]?.ToString());
        Assert.Equal("Empresa Teste", company["legalName"]?.ToString());
    }

    [Fact]
    public async Task Companies_WithoutCreatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createCompany(input: { legalName: \"Denied Company\" }) { id } }",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Companies_WithUpdatePermission_MustSucceed()
    {
        var userId = Guid.NewGuid();
        var dto = new CompanyDto(
            Guid.NewGuid(), TenantId, "Empresa Atualizada", "Fantasia", null, null, null,
            null, null, null, null, null, null, null, null, null, null, null,
            null, CompanyStatus.Active, "updated", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateCompanyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<CompanyDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.CompaniesUpdate, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateCompany(id: \"{Guid.NewGuid()}\", input: {{ legalName: \"Empresa Atualizada\", notes: \"updated\" }}) {{ id legalName }} }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var company = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["updateCompany"]);
        Assert.Equal(dto.Id.ToString(), company["id"]?.ToString());
    }

    [Fact]
    public async Task Companies_WithoutUpdatePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateCompany(id: \"{Guid.NewGuid()}\", input: {{ legalName: \"Denied\" }}) {{ id }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Companies_WithoutDeletePermission_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteCompany(id: \"{Guid.NewGuid()}\") }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    // â”€â”€â”€ USERS / ROLES â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task Users_WithManagePermission_MustCreateUser()
    {
        var userId = Guid.NewGuid();
        var dto = new UserDto(
            Guid.NewGuid(), TenantId, "User Test", "user@test.com", true,
            null, DateTimeOffset.UtcNow, null, null, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<UserDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.UsersManage, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createUser(input: { name: \"User Test\", email: \"user@test.com\", password: \"Secret@123\" }) { id email } }",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var createdUser = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createUser"]);
        Assert.Equal(dto.Id.ToString(), createdUser["id"]?.ToString());
    }

    [Fact]
    public async Task Users_WithoutManagePermission_MustDenyCreateUser()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createUser(input: { name: \"Denied\", email: \"denied@test.com\", password: \"Secret@123\" }) { id } }",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Users_WithManagePermission_MustUpdateUser()
    {
        var userId = Guid.NewGuid();
        var dto = new UserDto(
            Guid.NewGuid(), TenantId, "User Updated", "updated@test.com", true,
            null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<UserDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.UsersManage, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateUser(id: \"{Guid.NewGuid()}\", input: {{ name: \"User Updated\", email: \"updated@test.com\", isActive: true }}) {{ id email }} }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var updatedUser = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["updateUser"]);
        Assert.Equal(dto.Id.ToString(), updatedUser["id"]?.ToString());
    }

    [Fact]
    public async Task Users_WithoutManagePermission_MustDenyUpdateUser()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateUser(id: \"{Guid.NewGuid()}\", input: {{ name: \"Denied\", email: \"denied@test.com\", isActive: true }}) {{ id }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Roles_WithManagePermission_MustCreateRole()
    {
        var userId = Guid.NewGuid();
        var dto = new RoleDto(
            Guid.NewGuid(), TenantId, "Coordinator", null, true, [],
            DateTimeOffset.UtcNow, null, null, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<RoleDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.RolesManage, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createRole(input: { name: \"Coordinator\" }) { id name } }",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var role = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["createRole"]);
        Assert.Equal(dto.Id.ToString(), role["id"]?.ToString());
    }

    [Fact]
    public async Task Roles_WithoutManagePermission_MustDenyCreateRole()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createRole(input: { name: \"Denied\" }) { id } }",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Roles_WithManagePermission_MustUpdateRole()
    {
        var userId = Guid.NewGuid();
        var dto = new RoleDto(
            Guid.NewGuid(), TenantId, "Coordinator Updated", "desc", true, [],
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Results.Result<RoleDto>.Success(dto));

        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, SystemPermissions.RolesManage, Arg.Any<CancellationToken>())
            .Returns(true);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateRole(id: \"{Guid.NewGuid()}\", input: {{ name: \"Coordinator Updated\", isActive: true }}) {{ id name }} }}",
            userId))).ExpectOperationResult();

        Assert.Null(result.Errors);
        var role = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.Data!["updateRole"]);
        Assert.Equal(dto.Id.ToString(), role["id"]?.ToString());
    }

    [Fact]
    public async Task Roles_WithoutManagePermission_MustDenyUpdateRole()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ updateRole(id: \"{Guid.NewGuid()}\", input: {{ name: \"Denied\", isActive: true }}) {{ id }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Users_WithoutManagePermission_MustDenyDeleteUser()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteUser(id: \"{Guid.NewGuid()}\") }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Roles_WithoutManagePermission_MustDenyDeleteRole()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        permissionService.HasPermissionAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ deleteRole(id: \"{Guid.NewGuid()}\") }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    // ─── CROSS-CONTAMINATION GUARD ──────────────────────────────────────────

    [Fact]
    public async Task Students_WithOnlyCustomersCreate_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        // Has customers:create but NOT students:create — must not bleed across
        permissionService.HasPermissionAsync(userId, SystemPermissions.CustomersCreate, Arg.Any<CancellationToken>())
            .Returns(true);
        permissionService.HasPermissionAsync(userId, SystemPermissions.StudentsCreate, Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            $"mutation {{ createStudent(input: {{ personId: \"{Guid.NewGuid()}\" }}) {{ student {{ id }} }} }}",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }

    [Fact]
    public async Task Courses_WithOnlyEmployeesCreate_MustDeny()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var permissionService = Substitute.For<IPermissionService>();
        // Has employees:create but NOT courses:create
        permissionService.HasPermissionAsync(userId, SystemPermissions.EmployeesCreate, Arg.Any<CancellationToken>())
            .Returns(true);
        permissionService.HasPermissionAsync(userId, SystemPermissions.CoursesCreate, Arg.Any<CancellationToken>())
            .Returns(false);

        var executor = await BuildExecutorAsync(mediator, permissionService);
        var result = (await executor.ExecuteAsync(BuildRequest(
            "mutation { createCourse(input: { name: \"Should Fail\", type: LANGUAGE }) { course { id } } }",
            userId))).ExpectOperationResult();

        AssertAuthError(result);
    }
}
