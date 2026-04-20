using MyCRM.CRM.Application.DTOs;

namespace MyCRM.GraphQL.GraphQL.Scheduling;

public record ProfessionalPayload(ProfessionalDto Professional);
public record PatientPayload(PatientDto Patient);
public record ServicePayload(ServiceDto Service);
public record ProfessionalServicePayload(ProfessionalServiceDto ProfessionalService);
public record CommissionRulePayload(CommissionRuleDto CommissionRule);
public record ProfessionalSchedulePayload(ProfessionalScheduleDto ProfessionalSchedule);
public record AppointmentPayload(AppointmentDto Appointment);
public record AppointmentTaskPayload(AppointmentTaskDto AppointmentTask);

public record CreateAppointmentPayload(
    AppointmentDto Appointment,
    IReadOnlyList<AppointmentDto> RecurringAppointments,
    List<string> Warnings);
