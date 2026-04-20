using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.ProfessionalSchedules.UpdateProfessionalSchedule;

public sealed record UpdateProfessionalScheduleCommand(
    Guid Id,
    TimeSpan StartTime,
    TimeSpan EndTime,
    bool IsAvailable
) : IRequest<Result<ProfessionalScheduleDto>>;
