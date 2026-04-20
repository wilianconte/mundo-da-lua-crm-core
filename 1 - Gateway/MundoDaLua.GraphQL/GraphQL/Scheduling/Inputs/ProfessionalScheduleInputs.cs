namespace MyCRM.GraphQL.GraphQL.Scheduling.Inputs;

public record UpdateProfessionalScheduleInput(
    Guid Id,
    TimeSpan StartTime,
    TimeSpan EndTime,
    bool IsAvailable);
