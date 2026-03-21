namespace MyCRM.GraphQL.GraphQL.Customers.Inputs;

public record UpdateCustomerInput(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Document,
    string? Notes
);
