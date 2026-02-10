using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;
using Users.Entities;

namespace Users.Features.GetAllGroups;

public class GetAllGroupsQueryHandler : IQueryHandler<GetAllGroupsQuery, List<GroupDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllGroupsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GroupDto>>> Handle(
        GetAllGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var groups = await _context.Set<Group>()
            .Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .Select(g => new GroupDto(g.Id, g.Name))
            .ToListAsync(cancellationToken);

        return Result.Success(groups);
    }
}
