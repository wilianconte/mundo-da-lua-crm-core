using FluentValidation;

namespace MyCRM.Auth.Application.Commands.Roles.UpdateRolePermissions;

public sealed class UpdateRolePermissionsValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required.");

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("PermissionIds must not be null.");

        RuleForEach(x => x.PermissionIds)
            .NotEmpty().WithMessage("Each PermissionId must be a valid non-empty GUID.");
    }
}
