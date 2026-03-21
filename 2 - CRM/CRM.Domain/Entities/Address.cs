namespace MyCRM.CRM.Domain.Entities;

public sealed class Address
{
    public string Street { get; init; } = default!;
    public string? Number { get; init; }
    public string? Complement { get; init; }
    public string Neighborhood { get; init; } = default!;
    public string City { get; init; } = default!;
    public string State { get; init; } = default!;
    public string ZipCode { get; init; } = default!;
    public string Country { get; init; } = "BR";
}
