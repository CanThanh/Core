using BCrypt.Net;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Identity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Identity.Features.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
        ApplicationDbContext context,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Set<User>()
             .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponse>("Invalid username or password");
        }

        if (!user.IsActive)
        {
            return Result.Failure<LoginResponse>("User account is deactivated");
        }

        // Get user roles (will be implemented in Authorization module)
        var roles = new List<string> { "User" }; // Default role

        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshTokenString = _jwtService.GenerateRefreshToken();

        var refreshTokenExpiration = DateTime.UtcNow.AddDays(
            Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));

        var refreshToken = new Identity.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = refreshTokenExpiration
        };

        _context.Set<Identity.Entities.RefreshToken>().Add(refreshToken);

        user.LastLoginAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse(
            accessToken,
            refreshTokenString,
            refreshTokenExpiration,
            new UserInfo(user.Id, user.Username, user.Email, user.FullName)
        );

        return Result.Success(response);
    }
}
