namespace MyCRM.GraphQL.GraphQL.Customers.Inputs;

public record SetCustomerAddressInput(
    Guid CustomerId,
    string Street,
    string? Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country = "BR"
);
