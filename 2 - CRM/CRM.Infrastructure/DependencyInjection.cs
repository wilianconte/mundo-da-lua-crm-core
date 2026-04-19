using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;
using MyCRM.CRM.Infrastructure.Repositories;
using MyCRM.CRM.Infrastructure.Services;
using MyCRM.Shared.Kernel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyCRM.CRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CRMDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "crm")));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IStudentGuardianRepository, StudentGuardianRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IStudentCourseRepository, StudentCourseRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IReconciliationRepository, ReconciliationRepository>();
        services.AddScoped<ICrmTenantProvisioningService, CrmTenantProvisioningService>();

        // Agendamentos
        services.AddScoped<IProfessionalSpecialtyRepository, ProfessionalSpecialtyRepository>();
        services.AddScoped<IProfessionalSpecialtyLinkRepository, ProfessionalSpecialtyLinkRepository>();
        services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IProfessionalServiceRepository, ProfessionalServiceRepository>();
        services.AddScoped<ICommissionRuleRepository, CommissionRuleRepository>();
        services.AddScoped<IProfessionalScheduleRepository, ProfessionalScheduleRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentRecurrenceRepository, AppointmentRecurrenceRepository>();
        services.AddScoped<IAppointmentTaskRepository, AppointmentTaskRepository>();

        return services;
    }
}
