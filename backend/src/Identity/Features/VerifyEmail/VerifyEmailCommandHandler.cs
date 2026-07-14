using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Features.VerifyEmail;

public class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    private readonly ApplicationDbContext _context;

    public VerifyEmailCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<VerifyEmailResponse>> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return Result.Failure<VerifyEmailResponse>("Invalid verification request");
        }

        if (user.IsEmailVerified)
        {
            return Result.Success(new VerifyEmailResponse("Email is already verified", true));
        }

        // Check token
        if (user.EmailVerificationToken != request.Token)
        {
            return Result.Failure<VerifyEmailResponse>("Invalid verification token");
        }

        // Check expiration
        if (user.EmailVerificationTokenExpiresAt.HasValue &&
            user.EmailVerificationTokenExpiresAt.Value < DateTime.UtcNow)
        {
            return Result.Failure<VerifyEmailResponse>("Verification token has expired. Please request a new one.");
        }

        // Mark email as verified
        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new VerifyEmailResponse("Email verified successfully", true));
    }
}
