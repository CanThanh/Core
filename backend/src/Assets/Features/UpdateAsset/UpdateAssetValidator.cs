using FluentValidation;

namespace Assets.Features.UpdateAsset;

public class UpdateAssetValidator : AbstractValidator<UpdateAssetCommand>
{
    public UpdateAssetValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Asset ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase price must be greater than or equal to 0");

        RuleFor(x => x.DepreciationRate)
            .GreaterThanOrEqualTo(0).WithMessage("Depreciation rate must be greater than or equal to 0")
            .LessThanOrEqualTo(100).WithMessage("Depreciation rate must be less than or equal to 100");

        RuleFor(x => x.Status)
            .Must(s => new[] { "InUse", "Maintenance", "Broken", "Disposed" }.Contains(s))
            .WithMessage("Status must be one of: InUse, Maintenance, Broken, Disposed");
    }
}
