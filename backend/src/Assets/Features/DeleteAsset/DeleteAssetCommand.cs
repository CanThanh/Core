using BuildingBlocks.Common.Abstractions;

namespace Assets.Features.DeleteAsset;

public record DeleteAssetCommand(Guid Id) : ICommand;
