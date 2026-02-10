using FluentValidation;

namespace Assets.Features.CreateAsset;

public class CreateAssetValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Asset code is required")
            .MaximumLength(100).WithMessage("Asset code cannot exceed 100 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Asset name is required")
            .MaximumLength(255).WithMessage("Asset name cannot exceed 255 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.PurchaseDate)
            .NotEmpty().WithMessage("Purchase date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Purchase date cannot be in the future");

        RuleFor(x => x.PurchasePrice)
            .GreaterThan(0).WithMessage("Purchase price must be greater than 0");
    }
}
