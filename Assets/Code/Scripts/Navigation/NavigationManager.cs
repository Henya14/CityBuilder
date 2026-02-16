using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public interface ShortestPathStrategy
{
    List<GraphSearchNode<SelectableObject>> FindShortestPathToDestination(List<GraphSearchNode<SelectableObject>> adjacencyGraph, GraphNode<SelectableObject> source, GraphNode<SelectableObject> destination);
}

public class NavigationManager : MonoBehaviour
{
    [SerializeField] GameObject carPrefab;
    BuildingAdjacencyGraph adjacencyGraph = new BuildingAdjacencyGraph();
    List<SelectableObject> selectedObjects = new List<SelectableObject>();
    SelectableObject selectableObject;
    float timeSinceLastWeightRecalculation = 0f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastWeightRecalculation += Time.deltaTime;

        if (timeSinceLastWeightRecalculation >= 1f)
        {
            RecalculateWeightsForRoads(timeSinceLastWeightRecalculation);
            timeSinceLastWeightRecalculation = 0f;
        }
        if (Input.GetMouseButtonDown(0))
        {

            if (selectedObjects.Count >= 2)
            {
                DeselectObjects();
            }
            if (adjacencyGraph.GetGraphNodeForSelectableObject(selectableObject) != null)
            {
                selectedObjects.Add(selectableObject);
                selectableObject.ToggleHighlight(true);
                selectableObject.FreezeHighlight(true);
                if (selectedObjects.Count == 2)
                {
                    var start = adjacencyGraph.GetGraphNodeForSelectableObject(selectedObjects[0]);
                    var destination = adjacencyGraph.GetGraphNodeForSelectableObject(selectedObjects[1]);
                    List<GraphSearchNode<SelectableObject>> route;
                    FindShortestPathBeetweenTwoPoints(start, destination, out route);
                    route?.ForEach(r =>
                    {
                        r.GraphNode.Value.FreezeHighlight(false);
                        r.GraphNode.Value.ToggleHighlight(true);
                        r.GraphNode.Value.FreezeHighlight(true);
                        selectedObjects.Add(r.GraphNode.Value);
                    });
                    if (route != null)
                    {
                        List<GraphNode<SelectableObject>> nodes;
                        List<Direction> dirs;
                        GetNodesForRoute(route, out nodes);
                        GetDirectionsForNodes(nodes, out dirs);
                        //PlaceCarAtPosition(nodes[0].Value.GetRelativeClosestGridPosition(nodes[1].Value.GetGridPosition()), dirs);
                        Debug.Log(string.Join(",", dirs.Select(d => d.ToString())));

                    }


                }

            }

        }
    }

    public void ObjectSelected(SelectableObject selectedObject)
    {
        if (selectedObject.GetSelectableObjectType() == SelectableObjectType.Tile)
        {
            return;
        }
        selectableObject = selectedObject;

    }



    private void GetDirectionsForRoute(List<GraphSearchNode<SelectableObject>> route, out List<Direction> dirs)
    {
        List<GraphNode<SelectableObject>> nodes;
        GetNodesForRoute(route, out nodes);
        GetDirectionsForNodes(nodes, out dirs);
    }
    private void GetDirectionsForNodes(List<GraphNode<SelectableObject>> nodes, out List<Direction> dirs)
    {

        dirs = new List<Direction>();
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            var startNodePosition = nodes[i].Value.GetRelativeClosestGridPosition(nodes[i + 1].Value.GetGridPosition());
            var destinationNodePosition = nodes[i + 1].Value.GetRelativeClosestGridPosition(nodes[i].Value.GetGridPosition());
            var directionVector = destinationNodePosition - startNodePosition;
            Direction direction;
            if (directionVector.z > 0)
            {
                direction = Direction.North;
            }
            else if (directionVector.z < 0)
            {
                direction = Direction.South;
            }
            else if (directionVector.x > 0)
            {
                direction = Direction.East;
            }
            else
            {
                direction = Direction.West;
            }

            dirs.Add(direction);
        }
    }
    private void GetNodesForRoute(List<GraphSearchNode<SelectableObject>> route, out List<GraphNode<SelectableObject>> nodes)
    {
        nodes = new List<GraphNode<SelectableObject>>();
        nodes.AddRange(route?.Select(r => r.GraphNode));
    }

    public void FindShortestPathBeetweenTwoPoints(GraphNode<SelectableObject> start, GraphNode<SelectableObject> destination, out List<GraphSearchNode<SelectableObject>> route)
    {
        if (start == null || destination == null)
        {
            route = null;
            return;
        }
        var algo = new DijkstraAlgorithm();
        var searchNodes = adjacencyGraph.GetGraphSearchNodes().Select(sn =>
        {
            sn.StraightLineDistanceToDestination = (start.Value.GetGridPosition() - sn.GraphNode.Value.GetGridPosition()).magnitude;
            return sn;
        }).ToList();
        route = algo.FindShortestPathToDestination(searchNodes,
        start,
        destination);
    }

    public float GetDistanceBetweenTwoNodes(GraphNode<SelectableObject> start, GraphNode<SelectableObject> destination)
    {
        if (start == null || destination == null)
        {
            return float.MaxValue;
        }
        var algo = new DijkstraAlgorithm();
        var searchNodes = adjacencyGraph.GetGraphSearchNodes().Select(sn =>
        {
            sn.StraightLineDistanceToDestination = (start.Value.GetGridPosition() - sn.GraphNode.Value.GetGridPosition()).magnitude;
            return sn;
        }).ToList();
        var route = algo.FindShortestPathToDestination(searchNodes,
        start,
        destination);
        if (route == null) return float.MaxValue;
        var distance = route.Max(r => r.CostToStart);
        return (float)distance;
    }

    public void RecalculateWeightsForRoads(float timeSinceLastRecalculation)
    {
        adjacencyGraph.GraphNodes.Where(n => n.Value.GetSelectableObjectType() == SelectableObjectType.Road).ToList().ForEach(roadNode =>
        {
            var road = roadNode.Value.GetGameObject().GetComponent<Road>();
            road.RefreshWeight(timeSinceLastRecalculation);
            var newWeight = road.Weight;
            if (newWeight == GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT)
            {
                return;
            }
            roadNode.Connections.ForEach(c =>
            {
                if (c.Weight == GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT)
                {
                    return;
                }
                if (c.Destination == roadNode)
                {

                    c.Weight = (int)newWeight;
                }
            });

        });
        VisualizeConnections();
    }

    public void VisualizeConnections()
    {
        var allRoads = adjacencyGraph.GraphNodes.Where(n => n.Value.GetSelectableObjectType() == SelectableObjectType.Road).ToList();
        if (allRoads.Count == 0)
        {
            return;
        }
        var connectionsEnumerable = adjacencyGraph.GraphNodes.Where(n => n.Value.GetSelectableObjectType() == SelectableObjectType.Road).SelectMany(n => n.Connections);
        var maxWeight = connectionsEnumerable.Max(c => c.Weight);
        var connections = connectionsEnumerable.ToList();
        connections.ForEach(connection =>
        {
            var destination = connection.Destination;
            var source = connection.Source;
            var destinationRoad = destination.Value.GetGameObject().GetComponent<Road>();
            var sourceRoad = source.Value.GetGameObject().GetComponent<Road>();
            if (destinationRoad == null || sourceRoad == null)
            {
                return;
            }
            if (connection.Weight ==  GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT)
            {
                return;
            }
            var color = Color.Lerp(Color.green, Color.red, connection.Weight / destinationRoad.MaxWeight());
            //var randomOffset = new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f));
            var randomOffset = Vector3.zero;
            Debug.DrawLine(source.Value.GetGameObject().transform.position + randomOffset, destination.Value.GetGameObject().transform.position + randomOffset, color, 1.0f);
            // add arrows
                var direction = (destination.Value.GetGameObject().transform.position - source.Value.GetGameObject().transform.position).normalized;
                var arrowHeadPosition = destination.Value.GetGameObject().transform.position - direction * 0.5f + randomOffset;
                Debug.DrawLine(arrowHeadPosition, arrowHeadPosition + Quaternion.Euler(0, 150, 0) * direction * 0.2f, color, 1.0f);
                Debug.DrawLine(arrowHeadPosition, arrowHeadPosition + Quaternion.Euler(0, -150, 0) * direction * 0.2f, color, 1.0f);
        });
    }



    public void StartCarOnRoute(List<GraphSearchNode<SelectableObject>> route, out CarNavigation carNavigation)
    {
        carNavigation = null;
        //List<Direction> dirs;
        //GetDirectionsForRoute(route, out dirs);
        var getCurrentCarNumbersInGame = FindObjectsOfType<CarNavigation>().Count();
        if (getCurrentCarNumbersInGame < 100)
        {

            PlaceCarAtPosition(route[0].GraphNode.Value.GetGridPosition(), route, out carNavigation);
        }

    }
    public void PlaceCarAtPosition(Vector3Int gridPosition, List<GraphSearchNode<SelectableObject>> route, out CarNavigation carNavigation)
    {
        var car = Instantiate(carPrefab);
        var gridManager = route[0].GraphNode.Value.GetGridManager();
        var carGamePosition = gridManager.GetSelectionCenter(new List<Vector3Int> { gridPosition });
        carGamePosition.y = gridManager.GetGamePositionAndRotationForGridPosition(new Vector3Int(0, 1, 0)).Item1.y;
        car.transform.position = carGamePosition;
        carNavigation = car.GetComponent<CarNavigation>();
        carNavigation.InitializeRoute(route);
    }

    // public void PlaceCarAtPositionOLD(Vector3Int gridPosition, List<Direction> directions)
    // {
    //     var car = Instantiate(carPrefab);

    //     var gridManager = route[0].GraphNode.Value.GetGridManager();
    //     var carGamePosition = gridManager.GetSelectionCenter(new List<Vector3Int> { gridPosition });
    //     carGamePosition.y = gridManager.GetGamePositionAndRotationForGridPosition(new Vector3Int(0, 1, 0)).Item1.y;
    //     car.transform.position = carGamePosition;
    //     car.GetComponent<CarNavigation>().CurrentGridPosition = gridPosition;
    //     car.GetComponent<CarNavigation>().SetDirections(directions);
    // }

    public void DeselectObjects()
    {
        selectedObjects.ForEach(so =>
        {
            so.FreezeHighlight(false);
            so.ToggleHighlight(false);
        });

        selectedObjects.Clear();
    }

    public void AddBuilding(SelectableObject building, Dictionary<SelectableObject, NeighbourWeights> weights)
    {
        adjacencyGraph.AddBuilding(building, weights);
    }

    public List<GraphNode<SelectableObject>> WhereBuildings(Func<GraphNode<SelectableObject>, bool> predicate)
    {
        return adjacencyGraph.WhereGraphNodes(predicate).GraphNodes;
    }

    public GraphNode<SelectableObject> GetGraphNodeForSelectableObject(SelectableObject objectToSearch)
    {
        return adjacencyGraph.GetGraphNodeForSelectableObject(objectToSearch);
    }

    public BuildingAdjacencyGraph GetAdjacencyGraph()
    {
        return adjacencyGraph;
    }

}
