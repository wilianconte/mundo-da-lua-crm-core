using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAppointments;

public sealed class GetAppointmentsHandler : IRequestHandler<GetAppointmentsQuery, Result<IReadOnlyList<AppointmentDto>>>
{
    private readonly IAppointmentRepository _repository;

    public GetAppointmentsHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken ct)
    {
        var all = await _repository.GetAllAsync(ct);

        var filtered = all.AsEnumerable();
        if (request.ProfessionalId.HasValue)
            filtered = filtered.Where(a => a.ProfessionalId == request.ProfessionalId.Value);
        if (request.PatientId.HasValue)
            filtered = filtered.Where(a => a.PatientId == request.PatientId.Value);
        if (request.Status.HasValue)
            filtered = filtered.Where(a => (int)a.Status == request.Status.Value);
        if (request.Date.HasValue)
            filtered = filtered.Where(a => DateOnly.FromDateTime(a.StartDateTime) == request.Date.Value);

        var result = filtered.ToList().Adapt<IReadOnlyList<AppointmentDto>>();
        return Result<IReadOnlyList<AppointmentDto>>.Success(result);
    }
}
