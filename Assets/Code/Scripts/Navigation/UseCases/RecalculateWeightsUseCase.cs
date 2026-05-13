public class RecalculateWeightsUseCase<T>
{
    private readonly IGraphRepository<T> _graphRepository;
    private readonly IRoadDataProvider<T> _roadDataProvider;

    public RecalculateWeightsUseCase(IGraphRepository<T> graphRepository, IRoadDataProvider<T> roadDataProvider)
    {
        _graphRepository = graphRepository;
        _roadDataProvider = roadDataProvider;
    }

    public void Execute(float timeSinceLastRecalculation)
    {
        var allNodes = _roadDataProvider.GetAllRoads();
        foreach (var node in allNodes)
        {
            _roadDataProvider.RefreshWeight(node, timeSinceLastRecalculation);
            foreach (var connection in node.Connections)
            {
                if (connection.Weight == GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT)
                {
                    continue;
                }
                // Update connection weight based on destination road's weight
                var newWeight = (int)_roadDataProvider.GetRoadWeight(connection.Destination);
                if (newWeight <= 0)
                {
                    newWeight = 1;
                }
                connection.Weight = newWeight;
            }
        }
    }
}