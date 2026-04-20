using HotChocolate.Types;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class RecurrenceFrequencyType : EnumType<RecurrenceFrequency>
{
    protected override void Configure(IEnumTypeDescriptor<RecurrenceFrequency> descriptor)
    {
        descriptor.Name("RecurrenceFrequency");
        descriptor.Value(RecurrenceFrequency.Weekly).Name("WEEKLY");
        descriptor.Value(RecurrenceFrequency.Biweekly).Name("BIWEEKLY");
        descriptor.Value(RecurrenceFrequency.Monthly).Name("MONTHLY");
    }
}
