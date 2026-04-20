using Mapster;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.Mappings;

public sealed class AppointmentTaskMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AppointmentTask, AppointmentTaskDto>();
    }
}
