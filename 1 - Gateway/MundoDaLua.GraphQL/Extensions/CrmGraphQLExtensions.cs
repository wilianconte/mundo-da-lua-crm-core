using HotChocolate.Execution.Configuration;
using MyCRM.GraphQL.GraphQL.Customers;
using MyCRM.GraphQL.GraphQL.People;
using MyCRM.GraphQL.GraphQL.Companies;
using MyCRM.GraphQL.GraphQL.Students;
using MyCRM.GraphQL.GraphQL.Students.Types;
using MyCRM.GraphQL.GraphQL.StudentGuardians;
using MyCRM.GraphQL.GraphQL.Courses;
using MyCRM.GraphQL.GraphQL.StudentCourses;
using MyCRM.GraphQL.GraphQL.Employees;
using MyCRM.GraphQL.GraphQL.Enums;

namespace MyCRM.GraphQL.Extensions;

public static class CrmGraphQLExtensions
{
    public static IRequestExecutorBuilder AddCrmGraphQL(this IRequestExecutorBuilder builder) => builder
        // Customers
        .AddTypeExtension<CustomerQueries>()
        .AddTypeExtension<CustomerMutations>()
        .AddType<CustomerObjectType>()
        // People
        .AddTypeExtension<PersonQueries>()
        .AddTypeExtension<PersonMutations>()
        // Companies
        .AddTypeExtension<CompanyQueries>()
        .AddTypeExtension<CompanyMutations>()
        // Students
        .AddTypeExtension<StudentQueries>()
        .AddTypeExtension<StudentMutations>()
        .AddTypeExtension<StudentObjectTypeExtension>()
        .AddType<StudentEnrollmentStatusType>()
        .AddType<StudentCourseStatusType>()
        .AddType<StudentFilterType>()
        .AddType<StudentSortType>()
        // Student Guardians
        .AddTypeExtension<StudentGuardianQueries>()
        .AddTypeExtension<StudentGuardianMutations>()
        // Courses
        .AddTypeExtension<CourseQueries>()
        .AddTypeExtension<CourseMutations>()
        // Student Courses
        .AddTypeExtension<StudentCourseQueries>()
        .AddTypeExtension<StudentCourseMutations>()
        // Employees
        .AddTypeExtension<EmployeeQueries>()
        .AddTypeExtension<EmployeeMutations>();
}
