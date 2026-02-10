using FluentValidation;

namespace Users.Features.CreateGroup;

public class CreateGroupValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupValidator()
    {
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
