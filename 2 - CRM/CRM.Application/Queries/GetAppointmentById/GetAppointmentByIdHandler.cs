using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAppointmentById;

public sealed class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _repository;

    public GetAppointmentByIdHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AppointmentDto>> Handle(GetAppointmentByIdQuery request, CancellationToken ct)
    {
        var appointment = await _repository.GetByIdAsync(request.Id, ct);
        if (appointment is null)
            return Result<AppointmentDto>.Failure("APPOINTMENT_NOT_FOUND", "Appointment not found.");

        return Result<AppointmentDto>.Success(appointment.Adapt<AppointmentDto>());
    }
}
