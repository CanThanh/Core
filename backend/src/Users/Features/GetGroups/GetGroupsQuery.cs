using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Models;

namespace Users.Features.GetGroups;

public record GetGroupsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null
) : IQuery<PagedResult<GroupDto>>;

public record GroupDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int MemberCount,
    DateTime CreatedAt
);
