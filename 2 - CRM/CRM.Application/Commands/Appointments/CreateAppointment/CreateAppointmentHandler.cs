using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Appointments.CreateAppointment;

public sealed class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Result<CreateAppointmentResult>>
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IAppointmentRecurrenceRepository _recurrenceRepo;
    private readonly IProfessionalRepository _professionalRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly IProfessionalServiceRepository _professionalServiceRepo;
    private readonly IProfessionalScheduleRepository _scheduleRepo;
    private readonly ITenantService _tenant;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepo,
        IAppointmentRecurrenceRepository recurrenceRepo,
        IProfessionalRepository professionalRepo,
        IPatientRepository patientRepo,
        IServiceRepository serviceRepo,
        IProfessionalServiceRepository professionalServiceRepo,
        IProfessionalScheduleRepository scheduleRepo,
        ITenantService tenant)
    {
        _appointmentRepo = appointmentRepo;
        _recurrenceRepo = recurrenceRepo;
        _professionalRepo = professionalRepo;
        _patientRepo = patientRepo;
        _serviceRepo = serviceRepo;
        _professionalServiceRepo = professionalServiceRepo;
        _scheduleRepo = scheduleRepo;
        _tenant = tenant;
    }

    public async Task<Result<CreateAppointmentResult>> Handle(CreateAppointmentCommand request, CancellationToken ct)
    {
        var warnings = new List<string>();

        var professional = await _professionalRepo.GetByIdAsync(request.ProfessionalId, ct);
        if (professional is null)
            return Result<CreateAppointmentResult>.Failure("PROFESSIONAL_NOT_FOUND", "Professional not found.");
        if (professional.Status != ProfessionalStatus.Active)
            return Result<CreateAppointmentResult>.Failure("PROFESSIONAL_NOT_ACTIVE", "Professional must be Active to schedule (RN-068).");

        var patient = await _patientRepo.GetByIdAsync(request.PatientId, ct);
        if (patient is null)
            return Result<CreateAppointmentResult>.Failure("PATIENT_NOT_FOUND", "Patient not found.");
        if (patient.Status != PatientStatus.Active)
            return Result<CreateAppointmentResult>.Failure("PATIENT_NOT_ACTIVE", "Patient must be Active to schedule (RN-069).");

        var service = await _serviceRepo.GetByIdAsync(request.ServiceId, ct);
        if (service is null)
            return Result<CreateAppointmentResult>.Failure("SERVICE_NOT_FOUND", "Service not found.");
        if (!service.IsActive)
            return Result<CreateAppointmentResult>.Failure("SERVICE_NOT_ACTIVE", "Service must be active to schedule (RN-067).");

        var allPS = await _professionalServiceRepo.GetAllAsync(ct);
        var professionalService = allPS.FirstOrDefault(ps =>
            ps.ProfessionalId == request.ProfessionalId && ps.ServiceId == request.ServiceId && ps.IsActive);

        var durationMinutes = professionalService?.CustomDurationInMinutes ?? service.DefaultDurationInMinutes;
        var defaultPrice = professionalService?.CustomPrice ?? service.DefaultPrice;
        var price = request.OverridePrice ?? defaultPrice;

        if (price <= 0)
            return Result<CreateAppointmentResult>.Failure("APPOINTMENT_INVALID_PRICE", "Price must be greater than zero (RN-070).");

        var endDateTime = request.StartDateTime.AddMinutes(durationMinutes);

        Address? appointmentAddress = null;
        if (request.Type == AppointmentType.Domicilio)
        {
            if (request.Address is not null)
            {
                appointmentAddress = new Address
                {
                    Street = request.Address.Street,
                    Number = request.Address.Number,
                    Complement = request.Address.Complement,
                    Neighborhood = request.Address.Neighborhood,
                    City = request.Address.City,
                    State = request.Address.State,
                    ZipCode = request.Address.ZipCode,
                    Country = request.Address.Country
                };
            }
            else
            {
                warnings.Add("No address provided for Domicilio appointment — address must be filled later (RN-055).");
            }
        }

        var hasConflict = await _appointmentRepo.HasConflictAsync(
            request.ProfessionalId, request.StartDateTime, endDateTime, ct: ct);
        if (hasConflict)
            warnings.Add("Time conflict detected for this professional (RN-054).");

        var schedules = await _scheduleRepo.GetByProfessionalAsync(request.ProfessionalId, ct);
        var dayOfWeek = (int)request.StartDateTime.DayOfWeek;
        var daySchedule = schedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);
        if (daySchedule is null)
            warnings.Add($"No schedule configured for this professional on this day of week (RN-059).");
        else if (!daySchedule.IsAvailable)
            warnings.Add($"Professional is unavailable on this day of week (RN-059).");
        else
        {
            var startTime = request.StartDateTime.TimeOfDay;
            if (startTime < daySchedule.StartTime || startTime.Add(TimeSpan.FromMinutes(durationMinutes)) > daySchedule.EndTime)
                warnings.Add("Appointment time is outside the professional's working hours (RN-059).");
        }

        var appointment = Appointment.Create(
            _tenant.TenantId,
            request.ProfessionalId,
            request.PatientId,
            request.ServiceId,
            request.StartDateTime,
            endDateTime,
            request.Type,
            price,
            request.PaymentReceiver,
            request.PaymentMethodId,
            appointmentAddress,
            request.MeetingLink,
            notes: request.Notes);

        await _appointmentRepo.AddAsync(appointment, ct);
        await _appointmentRepo.SaveChangesAsync(ct);

        var recurringDtos = new List<AppointmentDto>();

        if (request.Recurrence is not null)
        {
            if (request.Recurrence.EndDate is null && request.Recurrence.MaxOccurrences is null)
                return Result<CreateAppointmentResult>.Failure("RECURRENCE_REQUIRES_END_CONDITION", "Either EndDate or MaxOccurrences must be provided (RN-057).");

            var recurrence = AppointmentRecurrence.Create(
                _tenant.TenantId,
                appointment.Id,
                request.Recurrence.Frequency,
                request.Recurrence.EndDate,
                request.Recurrence.MaxOccurrences);

            await _recurrenceRepo.AddAsync(recurrence, ct);
            await _recurrenceRepo.SaveChangesAsync(ct);

            var instances = GenerateRecurrenceInstances(
                appointment, recurrence, request, endDateTime, price, appointmentAddress);

            foreach (var instance in instances)
            {
                await _appointmentRepo.AddAsync(instance, ct);
                recurrence.IncrementOccurrences();
            }

            _recurrenceRepo.Update(recurrence);
            await _appointmentRepo.SaveChangesAsync(ct);
            await _recurrenceRepo.SaveChangesAsync(ct);

            recurringDtos = instances.Select(i => i.Adapt<AppointmentDto>()).ToList();
        }

        var dto = appointment!.Adapt<AppointmentDto>();

        return Result<CreateAppointmentResult>.Success(new CreateAppointmentResult(
            dto with { Warnings = warnings },
            recurringDtos,
            warnings));
    }

    private List<Appointment> GenerateRecurrenceInstances(
        Appointment parent,
        AppointmentRecurrence recurrence,
        CreateAppointmentCommand request,
        DateTime parentEndDateTime,
        decimal price,
        Address? address)
    {
        var instances = new List<Appointment>();
        var currentStart = parent.StartDateTime;
        var count = 0;

        while (true)
        {
            currentStart = GetNextOccurrence(currentStart, recurrence.Frequency);
            var currentEnd = currentStart.AddMinutes((parentEndDateTime - parent.StartDateTime).TotalMinutes);
            count++;

            if (recurrence.EndDate.HasValue && DateOnly.FromDateTime(currentStart) > recurrence.EndDate.Value)
                break;
            if (recurrence.MaxOccurrences.HasValue && count > recurrence.MaxOccurrences.Value)
                break;

            var instance = Appointment.Create(
                _tenant.TenantId,
                request.ProfessionalId,
                request.PatientId,
                request.ServiceId,
                currentStart,
                currentEnd,
                request.Type,
                price,
                request.PaymentReceiver,
                request.PaymentMethodId,
                address,
                request.MeetingLink,
                recurrenceId: recurrence.Id,
                notes: request.Notes);

            instances.Add(instance);
        }

        return instances;
    }

    private static DateTime GetNextOccurrence(DateTime current, RecurrenceFrequency frequency) => frequency switch
    {
        RecurrenceFrequency.Weekly => current.AddDays(7),
        RecurrenceFrequency.Biweekly => current.AddDays(14),
        RecurrenceFrequency.Monthly => current.AddMonths(1),
        _ => current.AddDays(7)
    };
}
