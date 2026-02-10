using FluentValidation;

namespace Users.Features.AssignRolesToUser;

public class AssignRolesToUserValidator : AbstractValidator<AssignRolesToUserCommand>
{
    public AssignRolesToUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.RoleIds)
            .NotNull()
            .WithMessage("RoleIds cannot be null.");
    }
}
