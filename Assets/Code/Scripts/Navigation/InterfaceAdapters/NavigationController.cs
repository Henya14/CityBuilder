
using System.Collections.Generic;
using System.Linq;

public class NavigationController<T>
{
    private readonly FindShortestPathUseCase<T> _findShortestPathUseCase;
    private readonly SpawnCarUseCase<T> _spawnCarUseCase;
    private readonly RecalculateWeightsUseCase<T> _recalculateWeightsUseCase;
    private readonly GetDistanceUseCase<T> _getDistanceUseCase;
    private readonly AddNodeToGraphUseCase<T> _addNodeToGraphUseCase;

    private readonly IGraphRepository<T> _graphRepository;

    public NavigationController(
        FindShortestPathUseCase<T> findShortestPathUseCase,
        SpawnCarUseCase<T> spawnCarUseCase,
        RecalculateWeightsUseCase<T> recalculateWeightsUseCase,
        GetDistanceUseCase<T> getDistanceUseCase,
        IGraphRepository<T> graphRepository,
        AddNodeToGraphUseCase<T> addNodeToGraphUseCase)
    {
        _findShortestPathUseCase = findShortestPathUseCase;
        _spawnCarUseCase = spawnCarUseCase;
        _recalculateWeightsUseCase = recalculateWeightsUseCase;
        _getDistanceUseCase = getDistanceUseCase;
        _graphRepository = graphRepository;
        _addNodeToGraphUseCase = addNodeToGraphUseCase;
    }

    public FindRouteResponse<T> FindShortestPath(FindRouteRequest<T> request)
    {
        return _findShortestPathUseCase.Execute(request);
    }

    public CarNavigation SpawnCar(FindRouteResponse<T> request)
    {
        return _spawnCarUseCase.Execute(request);
    }

    public float GetDistance(GraphNode<T> start, GraphNode<T> end)
    {
        var graphSearchNodes = _graphRepository.GetGraphSearchNodes(start);
        return _getDistanceUseCase.Execute(start, end, graphSearchNodes);
    }

    public void RecalculateWeights(float timeSinceLastRecalculation)
    {
        _recalculateWeightsUseCase.Execute(timeSinceLastRecalculation);
    }

    public GraphNode<T> GetNode(T node)
    {
        return _graphRepository.GetNode(node);
    }

    public GraphConnection<GraphNode<T>> GetConnection(GraphNode<T> from, GraphNode<T> to)
    {
        return _graphRepository.GetConnection(from, to);
    }

    public IReadOnlyList<GraphNode<T>> QueryGraphNodes(System.Func<GraphNode<T>, bool> predicate)
    {
        return _graphRepository.Query(predicate);
    }

    public IReadOnlyList<GraphSearchNode<T>> GetGraphSearchNodes(GraphNode<T> startNode)
    {
        return _graphRepository.GetGraphSearchNodes(startNode);
    }

    public GraphNode<T> AddBuilding(
            T building,
            Position position,
            Dictionary<T, NeighbourWeights> weights)
    {
        return _addNodeToGraphUseCase.Execute(building, position, weights);
       
    }

    public void RemoveBuilding(T building)
    {
        _graphRepository.RemoveNode(building);
    }
}