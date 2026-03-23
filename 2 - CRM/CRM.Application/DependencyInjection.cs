using MyCRM.CRM.Application.Mappings;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using MyCRM.Shared.Kernel.Behaviors;

namespace MyCRM.CRM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomersApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(assembly);

        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assembly);
        new CustomerMappingConfig().Register(config);
        new PersonMappingConfig().Register(config);
        new CompanyMappingConfig().Register(config);
        new StudentMappingConfig().Register(config);
        new StudentGuardianMappingConfig().Register(config);
        new CourseMappingConfig().Register(config);
        new StudentCourseMappingConfig().Register(config);

        return services;
    }
}
