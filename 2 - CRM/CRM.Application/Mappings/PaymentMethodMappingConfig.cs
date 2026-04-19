using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using Mapster;

namespace MyCRM.CRM.Application.Mappings;

public sealed class PaymentMethodMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PaymentMethod, PaymentMethodDto>();
    }
}
