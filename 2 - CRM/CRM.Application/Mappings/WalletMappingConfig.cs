using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using Mapster;

namespace MyCRM.CRM.Application.Mappings;

public sealed class WalletMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Wallet, WalletDto>()
            .Map(dest => dest.Balance, src => src.InitialBalance);
    }
}
