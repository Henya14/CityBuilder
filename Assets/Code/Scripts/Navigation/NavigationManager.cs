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
    GridManager gridManager;
    // Start is called before the first frame update
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ObjectSelected(SelectableObject selectedObject)
    {
        if (selectedObject.GetSelectableObjectType() == SelectableObjectType.Tile)
        {
            return;
        }
        if (selectedObjects.Count >= 2)
        {
            DeselectObjects();
        }
        if (adjacencyGraph.GetGraphNodeAtGridPosition(selectedObject.GetGridPosition()) != null)
        {
            selectedObjects.Add(selectedObject);
            selectedObject.ToggleHighlight(true);
            selectedObject.FreezeHighlight(true);
            if (selectedObjects.Count == 2)
            {

                var algo = new DijkstraAlgorithm();
                var start = adjacencyGraph.GetGraphNodeForSelectableObject(selectedObjects[0]);
                var destination = adjacencyGraph.GetGraphNodeForSelectableObject(selectedObjects[1]);
                var searchNodes = adjacencyGraph.GetGraphSearchNodes().Select(sn =>
                {
                    sn.StraightLineDistanceToDestination = (start.Value.GetGridPosition() - sn.GraphNode.Value.GetGridPosition()).magnitude;
                    return sn;
                }).ToList();
                var route = algo.FindShortestPathToDestination(searchNodes,
                start,
                destination);
                route?.ForEach(r =>
                {
                    r.GraphNode.Value.FreezeHighlight(false);
                    r.GraphNode.Value.ToggleHighlight(true);
                    r.GraphNode.Value.FreezeHighlight(true);
                    selectedObjects.Add(r.GraphNode.Value);
                });
                if (route != null)
                {
                    var nodes = new List<GraphNode<SelectableObject>>{start};
                    nodes.AddRange(route?.Select(r => r.GraphNode));
                    var dirs = new List<Direction>();
                    for (int i = 0; i < nodes.Count - 1; i++)
                    {
                        var startNodePosition =  nodes[i].Value.GetGridPosition();
                        var destinationNodePosition = nodes[i + 1].Value.GetRelativeClosestGridPosition(nodes[i].Value.GetGridPosition());
                        var directionVector =  destinationNodePosition - startNodePosition;
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
                    PlaceCarAtPosition(nodes[0].Value.GetRelativeClosestGridPosition(nodes[1].Value.GetGridPosition()), dirs);
                    Debug.Log(string.Join(",", dirs.Select(d => d.ToString())));
                }


            }

        }
    }

    private void PlaceCarAtPosition(Vector3Int gridPosition, List<Direction> directions)
    {
        var car = Instantiate(carPrefab);
        

        var carGamePosition = gridManager.GetSelectionCenter(new List<Vector3Int> { gridPosition });
        carGamePosition.y = gridManager.GetGamePositionForGridPosition(gridPosition).y;
        carGamePosition.z += gridManager.tileSize / 4;
        car.transform.position = carGamePosition;
        car.GetComponent<CarNavigation>().CurrentGridPosition = gridPosition;
        car.GetComponent<CarNavigation>().SetDirections(directions);
        car.GetComponent<CarNavigation>().Starta();
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

}
