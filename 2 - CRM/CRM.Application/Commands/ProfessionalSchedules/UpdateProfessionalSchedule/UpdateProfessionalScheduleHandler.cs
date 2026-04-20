using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.ProfessionalSchedules.UpdateProfessionalSchedule;

public sealed class UpdateProfessionalScheduleHandler : IRequestHandler<UpdateProfessionalScheduleCommand, Result<ProfessionalScheduleDto>>
{
    private readonly IProfessionalScheduleRepository _scheduleRepo;

    public UpdateProfessionalScheduleHandler(IProfessionalScheduleRepository scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<Result<ProfessionalScheduleDto>> Handle(UpdateProfessionalScheduleCommand request, CancellationToken ct)
    {
        var schedule = await _scheduleRepo.GetByIdAsync(request.Id, ct);
        if (schedule is null)
            return Result<ProfessionalScheduleDto>.Failure("PROFESSIONAL_SCHEDULE_NOT_FOUND", "Professional schedule not found.");

        schedule.Update(request.StartTime, request.EndTime, request.IsAvailable);
        _scheduleRepo.Update(schedule);
        await _scheduleRepo.SaveChangesAsync(ct);

        return Result<ProfessionalScheduleDto>.Success(schedule.Adapt<ProfessionalScheduleDto>());
    }
}
