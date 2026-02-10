using FluentValidation;

namespace Menus.Features.AssignMenuPermissionsToRole;

public class AssignMenuPermissionsToRoleValidator
    : AbstractValidator<AssignMenuPermissionsToRoleCommand>
{
    public AssignMenuPermissionsToRoleValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required");

        RuleFor(x => x.MenuPermissions)
            .NotNull().WithMessage("Menu permissions list is required");

        RuleForEach(x => x.MenuPermissions).ChildRules(menuPerm =>
        {
            menuPerm.RuleFor(x => x.MenuId)
                .NotEmpty().WithMessage("Menu ID is required");

            menuPerm.RuleFor(x => x.PermissionType)
                .NotEmpty().WithMessage("Permission type is required")
                .Must(BeValidPermissionType)
                .WithMessage("Permission type must be View, Create, Edit, or Delete");
        });
    }

    private bool BeValidPermissionType(string permissionType)
    {
        var validTypes = new[] { "View", "Create", "Edit", "Delete" };
        return validTypes.Contains(permissionType);
    }
}
