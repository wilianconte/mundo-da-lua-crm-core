using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Customers.Inputs;

public record CreateCustomerInput(string Name, string Email, CustomerType Type, string? Phone, string? Document);
