using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Professionals.UpdateProfessionalStatus;

public sealed class UpdateProfessionalStatusHandler : IRequestHandler<UpdateProfessionalStatusCommand, Result<ProfessionalDto>>
{
    private readonly IProfessionalRepository _professionalRepo;
    private readonly IWalletRepository _walletRepo;
    private readonly ITenantService _tenant;

    public UpdateProfessionalStatusHandler(
        IProfessionalRepository professionalRepo,
        IWalletRepository walletRepo,
        ITenantService tenant)
    {
        _professionalRepo = professionalRepo;
        _walletRepo       = walletRepo;
        _tenant           = tenant;
    }

    public async Task<Result<ProfessionalDto>> Handle(UpdateProfessionalStatusCommand request, CancellationToken ct)
    {
        var professional = await _professionalRepo.GetByIdAsync(request.Id, ct);
        if (professional is null)
            return Result<ProfessionalDto>.Failure("PROFESSIONAL_NOT_FOUND", "Professional not found.");

        switch (request.TargetStatus)
        {
            case ProfessionalStatus.Active:
                if (professional.Status != ProfessionalStatus.Draft && professional.Status != ProfessionalStatus.Inactive)
                    return Result<ProfessionalDto>.Failure(
                        "PROFESSIONAL_INVALID_TRANSITION",
                        $"Cannot transition from {professional.Status} to Active.");

                // Create Wallet automatically on Draft → Active (RN-061)
                if (professional.WalletId == null)
                {
                    var wallet = Wallet.Create(_tenant.TenantId, $"Profissional #{professional.Id}");
                    await _walletRepo.AddAsync(wallet, ct);
                    await _walletRepo.SaveChangesAsync(ct);
                    professional.Activate(wallet.Id);
                }
                else
                {
                    professional.Activate(professional.WalletId.Value);
                }
                break;

            case ProfessionalStatus.Inactive:
                if (professional.Status != ProfessionalStatus.Active)
                    return Result<ProfessionalDto>.Failure(
                        "PROFESSIONAL_INVALID_TRANSITION",
                        $"Cannot transition from {professional.Status} to Inactive.");
                professional.Deactivate();
                break;

            case ProfessionalStatus.Suspended:
                if (professional.Status != ProfessionalStatus.Active)
                    return Result<ProfessionalDto>.Failure(
                        "PROFESSIONAL_INVALID_TRANSITION",
                        $"Cannot transition from {professional.Status} to Suspended.");
                professional.Suspend();
                break;

            case ProfessionalStatus.Terminated:
                professional.Terminate();
                break;

            default:
                return Result<ProfessionalDto>.Failure(
                    "PROFESSIONAL_INVALID_TRANSITION", $"Status {request.TargetStatus} is not a valid target.");
        }

        _professionalRepo.Update(professional);
        await _professionalRepo.SaveChangesAsync(ct);

        return Result<ProfessionalDto>.Success(professional.Adapt<ProfessionalDto>());
    }
}
