using HotChocolate.Data.Filters;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Students.Types;

/// <summary>
/// Custom filter type for Student.
/// Note: enrollmentStatus filter is handled via query parameter, not via filter type
/// because it's a computed field based on the Courses collection.
/// </summary>
public sealed class StudentFilterType : FilterInputType<Student>
{
    protected override void Configure(IFilterInputTypeDescriptor<Student> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        // Standard fields (all filterable at database level)
        descriptor.Field(f => f.Id);
        descriptor.Field(f => f.PersonId);
        descriptor.Field(f => f.UnitId);
        descriptor.Field(f => f.Notes);
        descriptor.Field(f => f.CreatedAt);
        descriptor.Field(f => f.UpdatedAt);

        // Note: enrollmentStatus is intentionally excluded from filter type
        // because it's computed from the Courses collection. Use the
        // enrollmentStatus query parameter on GetStudents instead.
    }
}
