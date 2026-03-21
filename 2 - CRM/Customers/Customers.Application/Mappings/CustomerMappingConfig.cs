using MyCRM.Customers.Application.DTOs;
using MyCRM.Customers.Domain.Entities;
using Mapster;

namespace MyCRM.Customers.Application.Mappings;

public sealed class CustomerMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Customer, CustomerDto>()
            .Map(dest => dest.Address, src => src.Address == null ? null : src.Address.Adapt<AddressDto>());

        config.NewConfig<Address, AddressDto>();
    }
}
