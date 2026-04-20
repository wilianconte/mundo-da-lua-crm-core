using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.CommissionRules.CreateCommissionRule;

public sealed record CreateCommissionRuleCommand(
    decimal CompanyPercentage,
    Guid? ProfessionalId,
    Guid? ServiceId
) : IRequest<Result<CommissionRuleDto>>;
