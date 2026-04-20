using HotChocolate.Types;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class PaymentReceiverTypeEnumType : EnumType<PaymentReceiverType>
{
    protected override void Configure(IEnumTypeDescriptor<PaymentReceiverType> descriptor)
    {
        descriptor.Name("PaymentReceiverType");
        descriptor.Value(PaymentReceiverType.Company).Name("COMPANY");
        descriptor.Value(PaymentReceiverType.Professional).Name("PROFESSIONAL");
    }
}
