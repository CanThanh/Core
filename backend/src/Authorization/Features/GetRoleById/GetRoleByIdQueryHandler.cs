using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Features.GetRoleById;

public class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleDetailDto>
{
    private readonly ApplicationDbContext _context;

    public GetRoleByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RoleDetailDto>> Handle(
        GetRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var role = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role is null)
        {
            return Result.Failure<RoleDetailDto>("Role not found");
        }

        var dto = new RoleDetailDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt
        );

        return Result.Success(dto);
    }
}
