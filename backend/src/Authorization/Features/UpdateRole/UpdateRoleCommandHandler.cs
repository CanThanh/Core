using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Features.UpdateRole;

public class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, UpdateRoleResponse>
{
    private readonly ApplicationDbContext _context;

    public UpdateRoleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateRoleResponse>> Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role is null)
        {
            return Result.Failure<UpdateRoleResponse>("Role not found");
        }

        // Check if another role with same name exists
        var exists = await _context.Set<Role>()
            .AnyAsync(r => r.Name == request.Name && r.Id != request.Id, cancellationToken);

        if (exists)
        {
            return Result.Failure<UpdateRoleResponse>("Role with this name already exists");
        }

        role.Name = request.Name;
        role.Description = request.Description;
        role.IsActive = request.IsActive;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new UpdateRoleResponse(role.Id, role.Name);

        return Result.Success(response);
    }
}
