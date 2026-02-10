using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Features.ResetPassword;

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly ApplicationDbContext _context;

    public ResetPasswordCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ResetPasswordResponse>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return Result.Failure<ResetPasswordResponse>("Invalid reset token");
        }

        // Find valid reset token
        var resetToken = await _context.Set<PasswordResetToken>()
            .Where(t => t.UserId == user.Id
                && !t.IsUsed
                && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (resetToken == null)
        {
            return Result.Failure<ResetPasswordResponse>("Invalid or expired reset token");
        }

        // Verify token
        if (!BCrypt.Net.BCrypt.Verify(request.Token, resetToken.Token))
        {
            return Result.Failure<ResetPasswordResponse>("Invalid reset token");
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // Mark token as used
        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;

        // Invalidate all refresh tokens for security
        var refreshTokens = await _context.Set<Identity.Entities.RefreshToken>()
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var rt in refreshTokens)
        {
            rt.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ResetPasswordResponse("Password has been reset successfully"));
    }
}
