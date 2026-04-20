namespace MyCRM.GraphQL.GraphQL.Scheduling.Inputs;

public record CreateCommissionRuleInput(
    decimal CompanyPercentage,
    Guid? ProfessionalId,
    Guid? ServiceId);
