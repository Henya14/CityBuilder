
public class FindShortestPathUseCase<T>
{
    private readonly IShortestPathStrategy _shortestPathStrategy;
    private readonly IGraphRepository<T> _graphRepository;

    public FindShortestPathUseCase(
        IShortestPathStrategy shortestPathStrategy,
        IGraphRepository<T> graphRepository)
    {
        _shortestPathStrategy = shortestPathStrategy;
        _graphRepository = graphRepository;
    }

    public FindRouteResponse<T> Execute(FindRouteRequest<T> request)
    {

        if (request.Start == null || request.End == null || request.Start == request.End)
        {
            return new FindRouteResponse<T>(RouteResult<T>.Empty());
        }

        var searchNodes = _graphRepository.GetGraphSearchNodes(request.Start);
        if (searchNodes == null || searchNodes.Count == 0)
        {
            return new FindRouteResponse<T>(RouteResult<T>.Empty());
        }
        var routeResult = _shortestPathStrategy.FindShortestPathToDestination(
            searchNodes,
            request.Start,
            request.End);


        return new FindRouteResponse<T>(routeResult);
    }
}