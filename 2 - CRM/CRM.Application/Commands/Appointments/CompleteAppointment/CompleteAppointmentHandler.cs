using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Appointments.CompleteAppointment;

public sealed class CompleteAppointmentHandler : IRequestHandler<CompleteAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IProfessionalRepository _professionalRepo;
    private readonly ICommissionRuleRepository _commissionRuleRepo;
    private readonly IPaymentMethodRepository _paymentMethodRepo;
    private readonly IWalletRepository _walletRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly ITenantService _tenant;

    public CompleteAppointmentHandler(
        IAppointmentRepository appointmentRepo,
        IProfessionalRepository professionalRepo,
        ICommissionRuleRepository commissionRuleRepo,
        IPaymentMethodRepository paymentMethodRepo,
        IWalletRepository walletRepo,
        ITransactionRepository transactionRepo,
        ITenantService tenant)
    {
        _appointmentRepo = appointmentRepo;
        _professionalRepo = professionalRepo;
        _commissionRuleRepo = commissionRuleRepo;
        _paymentMethodRepo = paymentMethodRepo;
        _walletRepo = walletRepo;
        _transactionRepo = transactionRepo;
        _tenant = tenant;
    }

    public async Task<Result<AppointmentDto>> Handle(CompleteAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(request.Id, ct);
        if (appointment is null)
            return Result<AppointmentDto>.Failure("APPOINTMENT_NOT_FOUND", "Appointment not found.");

        if (appointment.Status != AppointmentStatus.Confirmed)
            return Result<AppointmentDto>.Failure("APPOINTMENT_INVALID_STATUS", "Only Confirmed appointments can be completed.");

        var professional = await _professionalRepo.GetByIdAsync(appointment.ProfessionalId, ct);
        var paymentMethod = await _paymentMethodRepo.GetByIdAsync(appointment.PaymentMethodId, ct);
        if (paymentMethod is null)
            return Result<AppointmentDto>.Failure("PAYMENT_METHOD_NOT_FOUND", "Payment method not found.");

        var companyWalletId = paymentMethod.WalletId;

        if (appointment.PaymentReceiver == PaymentReceiverType.Company)
        {
            var income = Transaction.Create(
                _tenant.TenantId,
                companyWalletId,
                TransactionType.Income,
                appointment.Price,
                $"Atendimento {appointment.Id}",
                request.CategoryId,
                appointment.PaymentMethodId,
                DateTime.UtcNow);

            await _transactionRepo.AddAsync(income, ct);
            await _transactionRepo.SaveChangesAsync(ct);
            appointment.Complete(income.Id);
        }
        else
        {
            if (professional is null || professional.WalletId is null)
                return Result<AppointmentDto>.Failure("PROFESSIONAL_WALLET_NOT_FOUND", "Professional wallet not found (RN-056).");

            var professionalWalletId = professional.WalletId.Value;

            var income = Transaction.Create(
                _tenant.TenantId,
                professionalWalletId,
                TransactionType.Income,
                appointment.Price,
                $"Atendimento {appointment.Id}",
                request.CategoryId,
                appointment.PaymentMethodId,
                DateTime.UtcNow);

            await _transactionRepo.AddAsync(income, ct);
            await _transactionRepo.SaveChangesAsync(ct);

            var effectiveRule = await _commissionRuleRepo.GetEffectiveRuleAsync(
                _tenant.TenantId, appointment.ProfessionalId, appointment.ServiceId, ct);

            if (effectiveRule is not null)
            {
                var valorEmpresa = appointment.Price * effectiveRule.CompanyPercentage / 100m;

                if (valorEmpresa > 0)
                {
                    var expense = Transaction.Create(
                        _tenant.TenantId,
                        professionalWalletId,
                        TransactionType.Expense,
                        valorEmpresa,
                        $"Repasse empresa - Atendimento {appointment.Id}",
                        request.CategoryId,
                        appointment.PaymentMethodId,
                        DateTime.UtcNow);

                    var companyIncome = Transaction.Create(
                        _tenant.TenantId,
                        companyWalletId,
                        TransactionType.Income,
                        valorEmpresa,
                        $"Repasse profissional - Atendimento {appointment.Id}",
                        request.CategoryId,
                        appointment.PaymentMethodId,
                        DateTime.UtcNow);

                    await _transactionRepo.AddAsync(expense, ct);
                    await _transactionRepo.AddAsync(companyIncome, ct);
                    await _transactionRepo.SaveChangesAsync(ct);
                }
            }

            appointment.Complete(income.Id);
        }

        _appointmentRepo.Update(appointment);
        await _appointmentRepo.SaveChangesAsync(ct);

        return Result<AppointmentDto>.Success(appointment.Adapt<AppointmentDto>() with { Warnings = new List<string>() });
    }
}
