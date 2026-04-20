using HotChocolate.Types;
using DomAppointmentType = MyCRM.CRM.Domain.Entities.AppointmentType;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class AppointmentTypeEnumType : EnumType<DomAppointmentType>
{
    protected override void Configure(IEnumTypeDescriptor<DomAppointmentType> descriptor)
    {
        descriptor.Name("AppointmentType");
        descriptor.Value(DomAppointmentType.Presencial).Name("PRESENCIAL");
        descriptor.Value(DomAppointmentType.Remoto).Name("REMOTO");
        descriptor.Value(DomAppointmentType.Domicilio).Name("DOMICILIO");
    }
}
