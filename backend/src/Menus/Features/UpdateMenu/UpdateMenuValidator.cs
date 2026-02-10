using FluentValidation;

namespace Menus.Features.UpdateMenu;

public class UpdateMenuValidator : AbstractValidator<UpdateMenuCommand>
{
    public UpdateMenuValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Menu ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Menu name is required")
            .MaximumLength(100).WithMessage("Menu name cannot exceed 100 characters");

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Icon cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Icon));

        RuleFor(x => x.Route)
            .MaximumLength(200).WithMessage("Route cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Route));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be >= 0");
    }
}
