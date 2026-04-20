using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Appointments.ResolveAppointmentTask;

public sealed class ResolveAppointmentTaskHandler : IRequestHandler<ResolveAppointmentTaskCommand, Result<AppointmentTaskDto>>
{
    private readonly IAppointmentTaskRepository _taskRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IProfessionalRepository _professionalRepo;
    private readonly IPaymentMethodRepository _paymentMethodRepo;
    private readonly ITenantService _tenant;

    public ResolveAppointmentTaskHandler(
        IAppointmentTaskRepository taskRepo,
        ITransactionRepository transactionRepo,
        IAppointmentRepository appointmentRepo,
        IProfessionalRepository professionalRepo,
        IPaymentMethodRepository paymentMethodRepo,
        ITenantService tenant)
    {
        _taskRepo = taskRepo;
        _transactionRepo = transactionRepo;
        _appointmentRepo = appointmentRepo;
        _professionalRepo = professionalRepo;
        _paymentMethodRepo = paymentMethodRepo;
        _tenant = tenant;
    }

    public async Task<Result<AppointmentTaskDto>> Handle(ResolveAppointmentTaskCommand request, CancellationToken ct)
    {
        var task = await _taskRepo.GetByIdAsync(request.TaskId, ct);
        if (task is null)
            return Result<AppointmentTaskDto>.Failure("APPOINTMENT_TASK_NOT_FOUND", "Appointment task not found.");

        if (task.Status != AppointmentTaskStatus.Pending && task.Status != AppointmentTaskStatus.Escalated)
            return Result<AppointmentTaskDto>.Failure("APPOINTMENT_TASK_INVALID_STATUS", $"Cannot resolve task with status {task.Status}.");

        if (request.ApplyPenalty)
        {
            if (request.PenaltyAmount <= 0)
                return Result<AppointmentTaskDto>.Failure("PENALTY_AMOUNT_INVALID", "Penalty amount must be greater than zero.");

            var appointment = await _appointmentRepo.GetByIdAsync(task.AppointmentId, ct);
            if (appointment is null)
                return Result<AppointmentTaskDto>.Failure("APPOINTMENT_NOT_FOUND", "Related appointment not found.");

            var professional = await _professionalRepo.GetByIdAsync(appointment.ProfessionalId, ct);
            if (professional is null || professional.WalletId is null)
                return Result<AppointmentTaskDto>.Failure("PROFESSIONAL_WALLET_NOT_FOUND", "Professional wallet not found.");

            var paymentMethod = await _paymentMethodRepo.GetByIdAsync(request.PaymentMethodId, ct);
            if (paymentMethod is null)
                return Result<AppointmentTaskDto>.Failure("PAYMENT_METHOD_NOT_FOUND", "Payment method not found.");

            var income = Transaction.Create(
                _tenant.TenantId,
                paymentMethod.WalletId,
                TransactionType.Income,
                request.PenaltyAmount,
                $"Multa NoShow - Atendimento {appointment.Id}",
                request.CategoryId,
                request.PaymentMethodId,
                DateTime.UtcNow);

            await _transactionRepo.AddAsync(income, ct);
            await _transactionRepo.SaveChangesAsync(ct);

            task.Complete($"ApplyPenalty: {request.PenaltyAmount:C}");
        }
        else
        {
            task.Complete("Absolve");
        }

        _taskRepo.Update(task);
        await _taskRepo.SaveChangesAsync(ct);

        return Result<AppointmentTaskDto>.Success(task.Adapt<AppointmentTaskDto>());
    }
}
