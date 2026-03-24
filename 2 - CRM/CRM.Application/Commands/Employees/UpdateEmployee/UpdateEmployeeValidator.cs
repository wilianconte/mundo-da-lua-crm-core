using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Employees.UpdateEmployee;

public sealed class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee Id is required.");

        RuleFor(x => x.EmployeeCode)
            .MaximumLength(50)
            .When(x => x.EmployeeCode is not null);

        RuleFor(x => x.Position)
            .MaximumLength(200)
            .When(x => x.Position is not null);

        RuleFor(x => x.Department)
            .MaximumLength(200)
            .When(x => x.Department is not null);

        RuleFor(x => x.ContractType)
            .MaximumLength(100)
            .When(x => x.ContractType is not null);

        RuleFor(x => x.WorkSchedule)
            .MaximumLength(200)
            .When(x => x.WorkSchedule is not null);

        RuleFor(x => x.WorkloadHours)
            .GreaterThan(0).WithMessage("WorkloadHours must be greater than 0.")
            .When(x => x.WorkloadHours is not null);

        RuleFor(x => x.PayrollNumber)
            .MaximumLength(50)
            .When(x => x.PayrollNumber is not null);

        RuleFor(x => x.CostCenter)
            .MaximumLength(100)
            .When(x => x.CostCenter is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
