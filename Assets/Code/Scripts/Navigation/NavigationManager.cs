using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    GridManager gridManager;
    SelectableObject selectableObject;
    // Start is called before the first frame update
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {
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
    public void StartCarOnRoute(List<GraphSearchNode<SelectableObject>> route)
    {
        List<Direction> dirs;
        GetDirectionsForRoute(route, out dirs);
        //PlaceCarAtPosition(route[0].GraphNode.Value.GetGridPosition(), dirs);
    }
    public void PlaceCarAtPosition(Vector3Int gridPosition, List<Direction> directions)
    {
        var car = Instantiate(carPrefab);


        var carGamePosition = gridManager.GetSelectionCenter(new List<Vector3Int> { gridPosition });
        carGamePosition.y = gridManager.GetGamePositionAndRotationForGridPosition(new Vector3Int(0, 1, 0)).Item1.y;
        car.transform.position = carGamePosition;
        car.GetComponent<CarNavigation>().CurrentGridPosition = gridPosition;
        car.GetComponent<CarNavigation>().SetDirections(directions);
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

}
