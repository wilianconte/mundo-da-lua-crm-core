using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllCommissionRules;

public sealed class GetAllCommissionRulesHandler : IRequestHandler<GetAllCommissionRulesQuery, Result<IReadOnlyList<CommissionRuleDto>>>
{
    private readonly ICommissionRuleRepository _repository;

    public GetAllCommissionRulesHandler(ICommissionRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CommissionRuleDto>>> Handle(GetAllCommissionRulesQuery request, CancellationToken ct)
    {
        var rules = await _repository.GetAllAsync(ct);
        var dtos = rules.Adapt<IReadOnlyList<CommissionRuleDto>>();
        return Result<IReadOnlyList<CommissionRuleDto>>.Success(dtos);
    }
}
