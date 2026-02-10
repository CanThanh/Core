using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Features.GetPermissions;

public class GetPermissionsQueryHandler : IQueryHandler<GetPermissionsQuery, List<PermissionDto>>
{
    private readonly ApplicationDbContext _context;

    public GetPermissionsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PermissionDto>>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _context.Set<Permission>()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .Select(p => new PermissionDto(
                p.Id,
                p.Name,
                p.Description,
                p.Module
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(permissions);
    }
}
