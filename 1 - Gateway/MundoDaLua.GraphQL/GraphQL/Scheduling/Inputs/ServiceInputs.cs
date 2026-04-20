namespace MyCRM.GraphQL.GraphQL.Scheduling.Inputs;

public record CreateServiceInput(
    string Name,
    decimal DefaultPrice,
    int DefaultDurationInMinutes,
    string? Description);

public record UpdateServiceInput(
    Guid Id,
    string Name,
    decimal DefaultPrice,
    int DefaultDurationInMinutes,
    string? Description);

public record DeactivateServiceInput(Guid Id);
