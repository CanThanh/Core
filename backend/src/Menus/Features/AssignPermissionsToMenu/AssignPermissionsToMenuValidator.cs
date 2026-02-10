using FluentValidation;

namespace Menus.Features.AssignPermissionsToMenu;

public class AssignPermissionsToMenuValidator
    : AbstractValidator<AssignPermissionsToMenuCommand>
{
    public AssignPermissionsToMenuValidator()
    {
        RuleFor(x => x.MenuId)
            .NotEmpty().WithMessage("Menu ID is required");

        RuleFor(x => x.Assignments)
            .NotNull().WithMessage("Assignments list is required");

        RuleForEach(x => x.Assignments).ChildRules(assignment =>
        {
            assignment.RuleFor(x => x.PermissionId)
                .NotEmpty().WithMessage("Permission ID is required");

            assignment.RuleFor(x => x.PermissionType)
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
