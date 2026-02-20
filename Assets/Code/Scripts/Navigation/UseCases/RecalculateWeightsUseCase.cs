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
                    return;
                }
                if (connection.Destination == node)
                {

                    connection.Weight = (int)_roadDataProvider.GetRoadWeight(node);
                }
            }
        }
    }
}