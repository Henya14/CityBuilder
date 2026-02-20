using System.Linq;
using UnityEngine;

public class ConnectionVisualizer : MonoBehaviour, IGraphConnectionVizualizer
{
    private NavigationGraph<SelectableObject> _navigationGraph;

    public void Init(NavigationGraph<SelectableObject> navigationGraph, IRoadDataProvider<SelectableObject> roadWeightProvider)
    {
        _navigationGraph = navigationGraph;
    }

    public void VisualizeConnections()
    {
        if (_navigationGraph == null)
        {
            return;
        }
        var allRoads = _navigationGraph.Where(n => n.Value.GetSelectableObjectType() == SelectableObjectType.Road);
        if (allRoads.Count == 0)
        {
            return;
        }
        var connectionsEnumerable = _navigationGraph.Where(n => n.Value.GetSelectableObjectType() == SelectableObjectType.Road).AsEnumerable().SelectMany(n => n.Connections);
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
}