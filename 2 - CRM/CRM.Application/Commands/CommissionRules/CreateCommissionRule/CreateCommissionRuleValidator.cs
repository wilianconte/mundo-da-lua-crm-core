using FluentValidation;

namespace MyCRM.CRM.Application.Commands.CommissionRules.CreateCommissionRule;

public sealed class CreateCommissionRuleValidator : AbstractValidator<CreateCommissionRuleCommand>
{
    public CreateCommissionRuleValidator()
    {
        RuleFor(x => x.CompanyPercentage).InclusiveBetween(0, 100)
            .WithMessage("CompanyPercentage must be between 0 and 100 (RN-048).");
    }
}
