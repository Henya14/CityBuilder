using System.Collections.Generic;

public class AddNodeToGraphUseCase<T>
{
    private readonly IGraphRepository<T> _graphRepository;

    public AddNodeToGraphUseCase(IGraphRepository<T> graphRepository)
    {
        _graphRepository = graphRepository;
    }

    public GraphNode<T> Execute(T value, Position position, Dictionary<T, NeighbourWeights> weights)
    {
        var node = _graphRepository.AddNode(value, position);
        foreach (var neighbour in weights)
        {
            var neighbourNode = _graphRepository.GetNode(neighbour.Key);
            if (neighbourNode == default)
            {
                continue;
            }
            if (neighbour.Value.WeightFromNeighbour != GraphConnection<T>.NO_CONNECTION_WEIGHT)
            {
                node.AddConnection(neighbourNode, neighbour.Value.WeightFromNeighbour);
            }

            if (neighbour.Value.WeightToNeighbour != GraphConnection<T>.NO_CONNECTION_WEIGHT)
            {
                neighbourNode.AddConnection(node, neighbour.Value.WeightToNeighbour);
            }
        }
        ;
        return node;
    }
}