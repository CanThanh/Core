using FluentValidation;

namespace Users.Features.UpdateGroup;

public class UpdateGroupValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty()
            .WithMessage("GroupId is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Group name is required.")
            .MinimumLength(2)
            .WithMessage("Group name must be at least 2 characters.")
            .MaximumLength(100)
            .WithMessage("Group name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 500 characters.");
    }
}
