using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Features.CreateRole;

public class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, CreateRoleResponse>
{
    private readonly ApplicationDbContext _context;

    public CreateRoleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateRoleResponse>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Check if role with same name already exists
        var exists = await _context.Set<Role>()
            .AnyAsync(r => r.Name == request.Name, cancellationToken);

        if (exists)
        {
            return Result.Failure<CreateRoleResponse>("Role with this name already exists");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Role>().Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreateRoleResponse(role.Id, role.Name);

        return Result.Success(response);
    }
}
