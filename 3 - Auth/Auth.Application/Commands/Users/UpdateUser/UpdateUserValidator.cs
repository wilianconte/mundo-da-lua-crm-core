using FluentValidation;

namespace MyCRM.Auth.Application.Commands.Users.UpdateUser;

public sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.")
            .MaximumLength(254);

        RuleFor(x => x.Password)
            .MaximumLength(128)
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleForEach(x => x.RoleIds)
            .NotEmpty()
            .When(x => x.RoleIds is not null);
    }
}
