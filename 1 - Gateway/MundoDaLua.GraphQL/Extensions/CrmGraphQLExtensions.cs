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
using MyCRM.GraphQL.GraphQL.Financial;
using MyCRM.GraphQL.GraphQL.Scheduling;
using MyCRM.GraphQL.GraphQL.Scheduling.Types;

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
        .AddTypeExtension<EmployeeMutations>()
        // Financial
        .AddTypeExtension<WalletQueries>()
        .AddTypeExtension<WalletMutations>()
        .AddTypeExtension<CategoryQueries>()
        .AddTypeExtension<CategoryMutations>()
        .AddTypeExtension<PaymentMethodQueries>()
        .AddTypeExtension<PaymentMethodMutations>()
        .AddTypeExtension<TransactionQueries>()
        .AddTypeExtension<TransactionMutations>()
        // Scheduling
        .AddTypeExtension<SchedulingQueries>()
        .AddTypeExtension<SchedulingMutations>()
        .AddType<ProfessionalType>()
        .AddType<ProfessionalSpecialtyType>()
        .AddType<ProfessionalSpecialtyLinkType>()
        .AddType<PatientType>()
        .AddType<ServiceType>()
        .AddType<ProfessionalServiceType>()
        .AddType<CommissionRuleType>()
        .AddType<ProfessionalScheduleType>()
        .AddType<AppointmentObjectType>()
        .AddType<AppointmentRecurrenceType>()
        .AddType<AppointmentTaskObjectType>()
        .AddType<AppointmentConflictWarningType>()
        .AddType<ProfessionalStatusType>()
        .AddType<PatientStatusType>()
        .AddType<AppointmentStatusType>()
        .AddType<AppointmentTypeEnumType>()
        .AddType<AppointmentTaskStatusType>()
        .AddType<AppointmentTaskTypeEnumType>()
        .AddType<RecurrenceFrequencyType>()
        .AddType<PaymentReceiverTypeEnumType>();
}
