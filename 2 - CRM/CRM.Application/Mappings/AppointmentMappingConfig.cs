using Mapster;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.Mappings;

public sealed class AppointmentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Appointment, AppointmentDto>()
            .Map(dest => dest.Warnings, src => new List<string>())
            .Map(dest => dest.Address, src => src.Address != null
                ? new AddressDto(
                    src.Address.Street,
                    src.Address.Number,
                    src.Address.Complement,
                    src.Address.Neighborhood,
                    src.Address.City,
                    src.Address.State,
                    src.Address.ZipCode,
                    src.Address.Country)
                : null);
    }
}
