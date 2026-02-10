using BuildingBlocks.Common.Abstractions;

namespace Users.Features.GetAllGroups;

public record GetAllGroupsQuery() : IQuery<List<GroupDto>>;

public record GroupDto(Guid Id, string Name);
