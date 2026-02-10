using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Features.Register;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly ApplicationDbContext _context;

    public RegisterCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // Check if username already exists
        var usernameExists = await _context.Set<User>()
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (usernameExists)
        {
            return Result.Failure<RegisterResponse>("Username already exists");
        }

        // Check if email already exists
        var emailExists = await _context.Set<User>()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<RegisterResponse>("Email already exists");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true
        };

        _context.Set<User>().Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new RegisterResponse(user.Id, user.Username, user.Email);

        return Result.Success(response);
    }
}
