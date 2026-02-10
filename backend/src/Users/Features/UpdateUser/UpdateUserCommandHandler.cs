using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Users.Features.UpdateUser;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly ApplicationDbContext _context;

    public UpdateUserCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateUserResponse>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<UpdateUserResponse>($"User with ID {request.UserId} not found");
        }

        // Check if email is already used by another user
        var emailExists = await _context.Set<User>()
            .AnyAsync(u => u.Email == request.Email && u.Id != request.UserId, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<UpdateUserResponse>(
                $"Email '{request.Email}' is already in use by another user.");
        }

        user.Email = request.Email;
        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new UpdateUserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.FullName
        );

        return Result.Success(response);
    }
}
