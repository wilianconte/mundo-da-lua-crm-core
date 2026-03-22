namespace MyCRM.GraphQL.GraphQL.Companies.Inputs;

public record SetCompanyAddressInput(
    Guid CompanyId,
    string Street,
    string? Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country = "BR"
);
