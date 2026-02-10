using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Features.GetRoles;

public class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly ApplicationDbContext _context;

    public GetRolesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<RoleDto>>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await _context.Set<Role>()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(roles);
    }
}
