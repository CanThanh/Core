using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Models;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;
using Users.Entities;

namespace Users.Features.GetGroups;

public class GetGroupsQueryHandler : IQueryHandler<GetGroupsQuery, PagedResult<GroupDto>>
{
    private readonly ApplicationDbContext _context;

    public GetGroupsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<GroupDto>>> Handle(
        GetGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<Group>().AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(g =>
                g.Name.Contains(request.SearchTerm) ||
                (g.Description != null && g.Description.Contains(request.SearchTerm)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(g => g.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var groups = await query
            .OrderBy(g => g.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var groupIds = groups.Select(g => g.Id).ToList();

        // Get member counts for groups
        var memberCounts = await _context.Set<UserGroup>()
            .Where(ug => groupIds.Contains(ug.GroupId))
            .GroupBy(ug => ug.GroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GroupId, x => x.Count, cancellationToken);

        var groupDtos = groups.Select(g => new GroupDto(
            g.Id,
            g.Name,
            g.Description,
            g.IsActive,
            memberCounts.ContainsKey(g.Id) ? memberCounts[g.Id] : 0,
            g.CreatedAt
        )).ToList();

        var result = new PagedResult<GroupDto>
        {
            Items = groupDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result.Success(result);
    }
}
