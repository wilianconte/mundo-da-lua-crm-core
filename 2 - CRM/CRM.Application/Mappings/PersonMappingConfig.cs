using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using Mapster;

namespace MyCRM.CRM.Application.Mappings;

public sealed class PersonMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Person, PersonDto>();
    }
}
