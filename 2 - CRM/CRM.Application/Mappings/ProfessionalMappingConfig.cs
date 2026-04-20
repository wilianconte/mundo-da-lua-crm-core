using Mapster;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.Mappings;

public sealed class ProfessionalMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Professional, ProfessionalDto>();
    }
}
