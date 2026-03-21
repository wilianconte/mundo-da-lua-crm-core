using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using Mapster;

namespace MyCRM.CRM.Application.Mappings;

public sealed class CustomerMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Customer, CustomerDto>()
            .Map(dest => dest.Address, src => src.Address == null ? null : src.Address.Adapt<AddressDto>());

        config.NewConfig<Address, AddressDto>();
    }
}
