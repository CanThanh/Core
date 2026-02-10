using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Users.Features.CreateUser;

public class CreateUserCommandHandler
    : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly ApplicationDbContext _context;

    public CreateUserCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateUserResponse>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        // Check if username already exists
        var usernameExists = await _context.Set<User>()
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (usernameExists)
        {
            return Result.Failure<CreateUserResponse>($"Username '{request.Username}' already exists");
        }

        // Check if email already exists
        var emailExists = await _context.Set<User>()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<CreateUserResponse>($"Email '{request.Email}' already exists");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create new user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            IsActive = request.IsActive
        };

        _context.Set<User>().Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreateUserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.FullName
        );

        return Result.Success(response);
    }
}
