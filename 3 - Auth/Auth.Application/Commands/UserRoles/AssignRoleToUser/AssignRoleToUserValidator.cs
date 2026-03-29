using FluentValidation;

namespace MyCRM.Auth.Application.Commands.UserRoles.AssignRoleToUser;

public sealed class AssignRoleToUserValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
