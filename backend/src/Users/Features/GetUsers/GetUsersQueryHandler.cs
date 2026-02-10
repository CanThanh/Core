using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Models;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Users.Entities;

namespace Users.Features.GetUsers;

public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly ApplicationDbContext _context;

    public GetUsersQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<User>().AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u =>
                u.Username.Contains(request.SearchTerm) ||
                u.Email.Contains(request.SearchTerm) ||
                u.FullName.Contains(request.SearchTerm) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(request.SearchTerm)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (request.GroupId.HasValue)
        {
            var userIdsInGroup = _context.Set<UserGroup>()
                .Where(ug => ug.GroupId == request.GroupId.Value)
                .Select(ug => ug.UserId);

            query = query.Where(u => userIdsInGroup.Contains(u.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.Username)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(u => u.Id).ToList();

        // Get roles for users
        var userRoles = await _context.Set<UserRole>()
            .Include(ur => ur.Role)
            .Where(ur => userIds.Contains(ur.UserId))
            .ToListAsync(cancellationToken);

        // Get groups for users
        var userGroups = await _context.Set<UserGroup>()
            .Include(ug => ug.Group)
            .Where(ug => userIds.Contains(ug.UserId))
            .ToListAsync(cancellationToken);

        var userDtos = users.Select(u => new UserDto(
            u.Id,
            u.Username,
            u.Email,
            u.FullName,
            u.PhoneNumber,
            u.IsActive,
            u.LastLoginAt,
            u.CreatedAt,
            userRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.Role.Name).ToList(),
            userGroups.Where(ug => ug.UserId == u.Id).Select(ug => ug.Group.Name).ToList()
        )).ToList();

        var result = new PagedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result.Success(result);
    }
}
