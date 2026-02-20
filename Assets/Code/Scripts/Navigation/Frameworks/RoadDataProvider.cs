
using System.Collections.Generic;
using UnityEngine;

public class RoadDataProvider : IRoadDataProvider<SelectableObject>
{
    private readonly NavigationGraph<SelectableObject> _navigationGraph;

    public RoadDataProvider(NavigationGraph<SelectableObject> navigationGraph)
    {
        _navigationGraph = navigationGraph;
    }

    public float GetRoadWeight(GraphNode<SelectableObject> node)
    {
        var road = GetRoad(node);
        if (road == null)
        {
            return GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT;
        }
        return road.Weight;
    }

    public float GetMaxRoadWeight(GraphNode<SelectableObject> node)
    {
        var road = GetRoad(node);
        if (road == null)        {
            return GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT;
        }

        return road.MaxWeight();
    }

    public void RefreshWeight(GraphNode<SelectableObject> node, float timeDelta)
    {
        var road = GetRoad(node);
        if (road != null)
        {
            road.RefreshWeight(timeDelta);
        } 
        else
        {
            return;
        }
        if (road.Weight == GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT)
        {
            return;
        }
        node.Connections.ForEach(connection =>
        {
            if (connection.Weight == GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT)
            {
                return;
            }
            if (connection.Destination == node)
            {
                connection.Weight = (int)road.Weight;
            }
        });
    }
    private Road GetRoad(GraphNode<SelectableObject> node)
    {
        if (node?.Value == null) return null;
        var mono = node.Value as MonoBehaviour;
        if (mono == null) return null;
        return mono.GetComponent<Road>();
    }

    public IReadOnlyList<GraphNode<SelectableObject>> GetAllRoads()
    {
        return _navigationGraph.Where(n => n.Value.GetSelectableObjectType() == SelectableObjectType.Road);
    }
}