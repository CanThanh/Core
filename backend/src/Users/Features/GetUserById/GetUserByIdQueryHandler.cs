using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Users.Entities;
using Users.Features.GetUsers;

namespace Users.Features.GetUserById;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly ApplicationDbContext _context;

    public GetUserByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<UserDto>($"User with ID {request.UserId} not found.");
        }

        // Get roles for user
        var roles = await _context.Set<UserRole>()
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        // Get groups for user
        var groups = await _context.Set<UserGroup>()
            .Include(ug => ug.Group)
            .Where(ug => ug.UserId == user.Id)
            .Select(ug => ug.Group.Name)
            .ToListAsync(cancellationToken);

        var userDto = new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            user.IsActive,
            user.LastLoginAt,
            user.CreatedAt,
            roles,
            groups
        );

        return Result.Success(userDto);
    }
}
