using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Identity.Features.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        ApplicationDbContext context,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _context.Set<Entities.RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return Result.Failure<RefreshTokenResponse>("Invalid or expired refresh token");
        }

        // Get user roles
        var roles = new List<string> { "User" }; // Default role

        var accessToken = _jwtService.GenerateAccessToken(refreshToken.User, roles);
        var newRefreshTokenString = _jwtService.GenerateRefreshToken();

        var refreshTokenExpiration = DateTime.UtcNow.AddDays(
            Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));

        // Revoke old refresh token
        refreshToken.RevokedAt = DateTime.UtcNow;

        // Create new refresh token
        var newRefreshToken = new Entities.RefreshToken
        {
            UserId = refreshToken.UserId,
            Token = newRefreshTokenString,
            ExpiresAt = refreshTokenExpiration
        };

        _context.Set<Entities.RefreshToken>().Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new RefreshTokenResponse(
            accessToken,
            newRefreshTokenString,
            refreshTokenExpiration
        );

        return Result.Success(response);
    }
}
