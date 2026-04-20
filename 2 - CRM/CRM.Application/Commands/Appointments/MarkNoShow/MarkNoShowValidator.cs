using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Appointments.MarkNoShow;

public sealed class MarkNoShowValidator : AbstractValidator<MarkNoShowCommand>
{
    public MarkNoShowValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
