using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.CheckAppointmentConflicts;

public sealed class CheckAppointmentConflictsHandler : IRequestHandler<CheckAppointmentConflictsQuery, Result<IReadOnlyList<AppointmentDto>>>
{
    private readonly IAppointmentRepository _repository;

    public CheckAppointmentConflictsHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AppointmentDto>>> Handle(CheckAppointmentConflictsQuery request, CancellationToken ct)
    {
        var all = await _repository.GetAllAsync(ct);

        var activeStatuses = new[] { AppointmentStatus.Suggested, AppointmentStatus.Confirmed, AppointmentStatus.Rescheduled };

        var conflicts = all.Where(a =>
            a.ProfessionalId == request.ProfessionalId
            && activeStatuses.Contains(a.Status)
            && a.Id != request.ExcludeAppointmentId
            && a.StartDateTime < request.EndDateTime
            && a.EndDateTime > request.StartDateTime)
            .ToList();

        var dtos = conflicts.Adapt<IReadOnlyList<AppointmentDto>>();
        return Result<IReadOnlyList<AppointmentDto>>.Success(dtos);
    }
}
