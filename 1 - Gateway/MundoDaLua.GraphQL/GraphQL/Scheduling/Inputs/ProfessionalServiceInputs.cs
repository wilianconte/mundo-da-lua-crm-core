namespace MyCRM.GraphQL.GraphQL.Scheduling.Inputs;

public record CreateProfessionalServiceInput(
    Guid ProfessionalId,
    Guid ServiceId,
    decimal? CustomPrice,
    int? CustomDurationInMinutes);
