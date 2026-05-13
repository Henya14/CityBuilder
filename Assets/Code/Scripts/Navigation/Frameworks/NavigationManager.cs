using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{

    float timeSinceLastWeightRecalculation = 0f;

    private RoadDataProvider _roadDataProvider;
    private ConnectionVisualizer _connectionVisualizer;

    private NavigationController<SelectableObject> _controller;
    private NavigationGraph<SelectableObject> _graph;
    SelectableObject selectableObject;
    List<SelectableObject> selectedObjects = new List<SelectableObject>();
    [SerializeField] private CarSpawner carSpawner;



    public void Start()
    {
        // 1. Create Entity
        _graph = new NavigationGraph<SelectableObject>();
        _roadDataProvider = new RoadDataProvider(_graph);
        _connectionVisualizer = FindObjectOfType<ConnectionVisualizer>();
        // 2. Create Interface Adapter (implements port using entity)
        var graphRepo = new GraphRepositoryAdapter<SelectableObject>(_graph);

        // 3. Create Use Cases (depend on ports, not implementations)
        var pathStrategy = new DijkstraAlgorithm();
        var findPath = new FindShortestPathUseCase<SelectableObject>(pathStrategy, graphRepo);
        var getDistance = new GetDistanceUseCase<SelectableObject>(pathStrategy);
        var recalcWeights = new RecalculateWeightsUseCase<SelectableObject>(graphRepo, _roadDataProvider);
        var spawnCar = new SpawnCarUseCase<SelectableObject>(carSpawner);
        var addNodeToGraph = new AddNodeToGraphUseCase<SelectableObject>(graphRepo);

        // 4. Create Controller (orchestrates use cases)
        _controller = new NavigationController<SelectableObject>(
            findPath, spawnCar, recalcWeights, getDistance, graphRepo, addNodeToGraph);

        if (_connectionVisualizer != null)
            _connectionVisualizer.Init(_graph, _roadDataProvider);

    }

    public void Update()
    {
        timeSinceLastWeightRecalculation += Time.deltaTime;

        if (timeSinceLastWeightRecalculation >= 1f)
        {
            _controller.RecalculateWeights(timeSinceLastWeightRecalculation);
            timeSinceLastWeightRecalculation = 0f;
            _connectionVisualizer.VisualizeConnections();
        }
        HandleMouseEvents();
    }
    private void HandleMouseEvents()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (selectedObjects.Count >= 2)
            {
                DeselectObjects();
            }
            if (_graph.GetNode(selectableObject) != null)
            {
                selectedObjects.Add(selectableObject);
                selectableObject.ToggleHighlight(true);
                selectableObject.FreezeHighlight(true);
                if (selectedObjects.Count == 2)
                {
                    var start = _graph.GetNode(selectedObjects[0]);
                    var destination = _graph.GetNode(selectedObjects[1]);
                    List<GraphSearchNode<SelectableObject>> route;
                    var shortestPathRequest = new FindRouteRequest<SelectableObject> (start, destination);
                    route = _controller.FindShortestPath(shortestPathRequest).RouteResult.ShortestPath;
                    var shortestPathResponse = _controller.FindShortestPath(shortestPathRequest);
                    route?.ForEach(r =>
                    {
                        r.GraphNode.Value.FreezeHighlight(false);
                        r.GraphNode.Value.ToggleHighlight(true);
                        r.GraphNode.Value.FreezeHighlight(true);
                        selectedObjects.Add(r.GraphNode.Value);
                    });
                }

            }

        }
    }

    public void DeselectObjects()
    {
        selectedObjects.ForEach(so =>
        {
            so.FreezeHighlight(false);
            so.ToggleHighlight(false);
        });

        selectedObjects.Clear();
    }

    public void ObjectSelected(SelectableObject selectedObject)
    {
        if (selectedObject.GetSelectableObjectType() == SelectableObjectType.Tile)
        {
            return;
        }
        selectableObject = selectedObject;

    }

    public GraphNode<SelectableObject> GetGraphNodeForSelectableObject(SelectableObject value)
    {
        return _graph.GetNode(value);
    }

    public void AddBuilding(SelectableObject building, Position position, Dictionary<SelectableObject, NeighbourWeights> weights)
    {
        var node = _controller.AddBuilding(building, position, weights);
        foreach (var neighbour in weights)
        {
            var neighbourNode = _graph.GetAllNodes().AsEnumerable().FirstOrDefault(x => x.Value.Equals(neighbour.Key));
            if (neighbourNode == default)
            {
                continue;
            }
            if (neighbour.Value.WeightFromNeighbour != GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT)
            {
                if (node.Value.GetGameObject() != null && node.Value.GetGameObject().GetComponent<Road>() != null)
                {
                    var road = node.Value.GetGameObject().GetComponent<Road>();
                    road.baseWeight = neighbour.Value.WeightFromNeighbour;
                    road.Weight = road.baseWeight;
                }
            }

            if (neighbour.Value.WeightToNeighbour != GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT)
            {
                if (node.Value.GetGameObject() != null && node.Value.GetGameObject().GetComponent<Road>() != null)
                {
                    var road = node.Value.GetGameObject().GetComponent<Road>();
                    road.baseWeight = neighbour.Value.WeightToNeighbour;
                    road.Weight = road.baseWeight;
                }
            }
        }
    }

    public NavigationController<SelectableObject> GetNavigationController()
    {
        return _controller;
    }
}