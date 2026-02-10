using BuildingBlocks.Common.Results;
using MediatR;

namespace BuildingBlocks.Common.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
