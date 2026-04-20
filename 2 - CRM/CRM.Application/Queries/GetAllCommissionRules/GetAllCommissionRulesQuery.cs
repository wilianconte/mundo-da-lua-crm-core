using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllCommissionRules;

public record GetAllCommissionRulesQuery : IRequest<Result<IReadOnlyList<CommissionRuleDto>>>;
