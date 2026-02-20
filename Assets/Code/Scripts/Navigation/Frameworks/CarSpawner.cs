using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour, ICarFactory<SelectableObject>
{

    [SerializeField] GameObject carPrefab;

    public CarNavigation SpawnCar(List<GraphSearchNode<SelectableObject>> route)
    {
        var position = route[0].GraphNode.Position;
        var vector3Position = new Vector3(position.x, position.y, position.z);
        PlaceCarAtPosition(vector3Position, route, out CarNavigation carNavigation);
        return carNavigation;
    }

    private void PlaceCarAtPosition(Vector3 worldPosition, List<GraphSearchNode<SelectableObject>> route, out CarNavigation carNavigation)
    {
        var car = Instantiate(carPrefab);
        var carGamePosition = new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
        car.transform.position = carGamePosition;
        carNavigation = car.GetComponent<CarNavigation>();
        carNavigation.InitializeRoute(route);
    }
}