using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAppointmentTasks;

public sealed class GetAppointmentTasksHandler : IRequestHandler<GetAppointmentTasksQuery, Result<IReadOnlyList<AppointmentTaskDto>>>
{
    private readonly IAppointmentTaskRepository _repository;

    public GetAppointmentTasksHandler(IAppointmentTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AppointmentTaskDto>>> Handle(GetAppointmentTasksQuery request, CancellationToken ct)
    {
        var all = await _repository.GetAllAsync(ct);

        var filtered = all.AsEnumerable();
        if (request.AppointmentId.HasValue)
            filtered = filtered.Where(t => t.AppointmentId == request.AppointmentId.Value);
        if (request.Status.HasValue)
            filtered = filtered.Where(t => (int)t.Status == request.Status.Value);

        var result = filtered.ToList().Adapt<IReadOnlyList<AppointmentTaskDto>>();
        return Result<IReadOnlyList<AppointmentTaskDto>>.Success(result);
    }
}
