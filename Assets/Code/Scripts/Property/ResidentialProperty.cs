using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum CarDestinationType { Work, Store, Home };
struct CarStatus
{
    public bool isAtHome;
    public bool isAtWork;
    public bool isAtStore;
    public bool isMoving;
    public GraphNode<SelectableObject> workNode;
    public GraphNode<SelectableObject> shoppingNode;
    public CarNavigation carNavigation;
    public CarDestinationType destinationType;
    public void ResetBools()
    {
        isAtHome = true;
        isAtWork = false;
        isAtStore = false;
        isMoving = false;
    }
}
public class ResidentialProperty : AbstarctProperty
{
    float chanceOfSpawningCar = 0.8f;
    NavigationManager navigationManager;
    private NavigationController<SelectableObject> _navigationController;
    private CarSpawner _carSpawner;
    List<CarNavigation> cars = new List<CarNavigation>();
    List<CarStatus> carsStatuses = new List<CarStatus>();

    int numberOfCars = 0;
    // Start is called before the first frame update
    protected override void Start()
    {
        this.PropertyType = PropertyType.Residental;
        navigationManager = FindObjectOfType<NavigationManager>();
        _carSpawner = FindObjectOfType<CarSpawner>();
        _navigationController = navigationManager.GetNavigationController();
        numberOfCars = UnityEngine.Random.Range(1, 3);
        for (int i = 0; i < numberOfCars; i++)
        {
            cars.Add(null);
            carsStatuses.Add(new CarStatus() { isAtHome = true, isAtStore = false, isAtWork = false, isMoving = false, workNode = GetRandomWorkNode(), shoppingNode = null });
        }
        TimeManager.On10MinutesChanged += CarSpawn;
        intensity = 5f;
        base.Start();
    }

    Coroutine CarSpawnCoroutineInstance;
    IEnumerator CarSpawnCoroutine()
    {


        var carsInShops = carsStatuses.Where(cs => cs.isAtStore && !cs.isMoving).ToList();

        for (int i = 0; i < carsInShops.Count; i++)
        {
            var carStatus = carsInShops[i];
            if (carStatus.shoppingNode == null)
            {
                carStatus.shoppingNode = GetRandomStoreNode();
                yield return null;
            }
            var start = navigationManager.GetGraphNodeForSelectableObject(carStatus.shoppingNode.Value);
            if (carStatus.isAtStore && !carStatus.isMoving)
            {
                var route = new List<GraphSearchNode<SelectableObject>>();
                var home = navigationManager.GetGraphNodeForSelectableObject(gameObject.GetComponent<SelectionManager>());
                 var shortestPathRequest = new FindRouteRequest<SelectableObject>(start, home);
                var shortestPathResponse = _navigationController.FindShortestPath(shortestPathRequest);
                route = shortestPathResponse.IsSuccess ? shortestPathResponse.RouteResult.ShortestPath : null;
                yield return null;
                if (route != null)
                {
                    CarNavigation carNavigation = _carSpawner.SpawnCar(route);
                    if (carNavigation == null)
                    {
                        yield break;
                    }
                    yield return null;
                    carStatus.isAtStore = false;
                    carStatus.isMoving = true;
                    carStatus.carNavigation = carNavigation;
                    carStatus.destinationType = CarDestinationType.Home;
                    carsStatuses[carsStatuses.FindIndex(cs => cs.carNavigation == carNavigation)] = carStatus;
                    carNavigation.ReachedDestination += CarReachedDestination;
                }
            }
        }

        if (TimeManager.Hour < 6 || TimeManager.Hour > 20)
        {
            CarSpawnCoroutineInstance = default;
            yield break;
        }
        ;

        if (TimeManager.Hour >= 6 && TimeManager.Hour <= 18)
        {

            var start = navigationManager.GetGraphNodeForSelectableObject(gameObject.GetComponent<SelectionManager>());
            for (int i = 0; i < carsStatuses.Count; i++)
            {
                var rand = UnityEngine.Random.Range(0f, 1f);
                var carStatus = carsStatuses[i];
                if (carStatus.isAtHome && !carStatus.isMoving && rand < chanceOfSpawningCar)
                {
                    var route = new List<GraphSearchNode<SelectableObject>>();
                    if (carStatus.workNode == null)
                    {
                        carStatus.workNode = GetRandomWorkNode();
                        yield return null;
                    }
                    if (carStatus.workNode == null)
                    {
                        Debug.Log($"No work node found for {gameObject.name}");
                        continue;
                    }
                    ;
                    var shortestPathResponse = _navigationController.FindShortestPath(new FindRouteRequest<SelectableObject>(start, carStatus.workNode));
                    route = shortestPathResponse.IsSuccess ? shortestPathResponse.RouteResult.ShortestPath : null;
                    yield return null;
                    if (route != null)
                    {
                        CarNavigation carNavigation = null;
                        carNavigation = _carSpawner.SpawnCar(route);
                        if (carNavigation == null)
                        {
                           yield break;
                        }
                        yield return null;
                        carStatus.isAtHome = false;
                        carStatus.isMoving = true;
                        carStatus.carNavigation = carNavigation;
                        carStatus.destinationType = CarDestinationType.Work;
                        carsStatuses[i] = carStatus;
                        carNavigation.ReachedDestination += CarReachedDestination;
                    }
                }
            }
        }

        if (TimeManager.Hour >= 17 && TimeManager.Hour <= 21)
        {
            for (int i = 0; i < carsStatuses.Count; i++)
            {

                var carStatus = carsStatuses[i];
                if (!(carStatus.isAtHome || carStatus.isAtWork) || carStatus.isMoving)
                {
                    continue;
                }
                var start = carStatus.isAtHome ? navigationManager.GetGraphNodeForSelectableObject(gameObject.GetComponent<SelectionManager>()) : navigationManager.GetGraphNodeForSelectableObject(carStatus.workNode.Value);
                yield return null;
                var rand = UnityEngine.Random.Range(0f, 1f);
                if ((carStatus.isAtWork || carStatus.isAtStore) && !carStatus.isMoving && rand < chanceOfSpawningCar)
                {
                    var route = new List<GraphSearchNode<SelectableObject>>();
                    var randomForStore = UnityEngine.Random.Range(0f, 1f);
                    GraphNode<SelectableObject> destinationNode = null;
                    if (randomForStore < 0.5f)
                    {
                        destinationNode = GetRandomStoreNode();
                        yield return null;

                    }
                    else
                    {
                        destinationNode = navigationManager.GetGraphNodeForSelectableObject(gameObject.GetComponent<SelectionManager>());
                        yield return null;
                    }
                    if (destinationNode == null)
                    {
                        Debug.Log($"No destination node found for {gameObject.name}");
                        carStatus.ResetBools();
                        continue;
                    }
                    var shortestPathResponse = _navigationController.FindShortestPath(new FindRouteRequest<SelectableObject>(start, destinationNode));
                    route = shortestPathResponse.IsSuccess ? shortestPathResponse.RouteResult.ShortestPath : null;
                    yield return null;
                    if (route != null)
                    {
                        CarNavigation carNavigation = _carSpawner.SpawnCar(route);
                        if (carNavigation == null)
                        {
                            yield break;
                        }
                        yield return null;
                        carStatus.isAtWork = false;
                        carStatus.isMoving = true;
                        carStatus.carNavigation = carNavigation;
                        carStatus.destinationType = CarDestinationType.Home;
                        carsStatuses[i] = carStatus;
                        carNavigation.ReachedDestination += CarReachedDestination;
                    }
                }
            }

        }

        if (numberOfCars > 0)
        {
            var start = navigationManager.GetGraphNodeForSelectableObject(gameObject.GetComponent<SelectionManager>());
            var type = UnityEngine.Random.Range(1, 3);
            List<GraphNode<SelectableObject>> destinations = new List<GraphNode<SelectableObject>>();
            switch (type)
            {
                case 1:
                    destinations = GetStoreNodes();
                    break;
                case 2:
                    destinations = GetBuildingNodes();
                    break;
            }
            yield return null;
            List<GraphSearchNode<SelectableObject>> route = null;
            destinations = destinations.OrderBy(_ => Guid.NewGuid()).ToList();
            foreach (var dest in destinations)
            {
                var shortestPathRequest = new FindRouteRequest<SelectableObject>(start, dest);
                route = _navigationController.FindShortestPath(shortestPathRequest).RouteResult.ShortestPath;
                if (route != null)
                {
                    break;
                }
                yield return null;
            }
            if (route != null)
            {
                //navigationManager.StartCarOnRoute(route);
            }
        }

        CarSpawnCoroutineInstance = default;
        yield break;
    }

    private void CarSpawn()
    {

        if (CarSpawnCoroutineInstance != default)
        {
            return;
        }
        else
        {
            CarSpawnCoroutineInstance = StartCoroutine(CarSpawnCoroutine());
        }


    }

    private void CarReachedDestination(CarNavigation car)
    {
        var carStatus = carsStatuses.FirstOrDefault(cs => cs.carNavigation == car);
        if (carStatus.destinationType == CarDestinationType.Work)
        {
            carStatus.isAtWork = true;

        }
        else if (carStatus.destinationType == CarDestinationType.Store)
        {
            carStatus.isAtStore = true;
        }
        else if (carStatus.destinationType == CarDestinationType.Home)
        {
            carStatus.isAtHome = true;
        }
        carStatus.isMoving = false;
        var index = carsStatuses.FindIndex(cs => cs.carNavigation == car);
        carsStatuses[index] = carStatus;
    }
    List<GraphNode<SelectableObject>> GetStoreNodes()
    {
        var stores = _navigationController.QueryGraphNodes(gn => gn.Value.GetSelectableObjectType() == SelectableObjectType.ZoneBuilding && gn.Value.GetDescription().Contains("Shopping"));
        return stores.ToList();
    }

    List<GraphNode<SelectableObject>> GetIndustrialNodes()
    {
        var factories = _navigationController.QueryGraphNodes(gn => gn.Value.GetSelectableObjectType() == SelectableObjectType.ZoneBuilding && gn.Value.GetDescription().Contains("Industrial"));
        return factories.ToList();
    }

    List<GraphNode<SelectableObject>> GetBuildingNodes()
    {
        var buildings = _navigationController.QueryGraphNodes(gn => gn.Value.GetSelectableObjectType() == SelectableObjectType.Building);
        return buildings.ToList();
    }

    private GraphNode<SelectableObject> GetRandomWorkNode()
    {
        var workPlaces = GetIndustrialNodes();
        var homeNode = navigationManager.GetGraphNodeForSelectableObject(gameObject.GetComponent<SelectionManager>());
        if (homeNode == null)
        {
            Debug.Log($"No home node found for {gameObject.name}");
            return null;
        }
        var workPlacesOrderedByDistance = workPlaces
        .Select(wp => new { Node = wp, Distance = _navigationController.GetDistance(homeNode, wp) })
        .OrderBy(wp => wp.Distance)
        .ToList();
        if (workPlacesOrderedByDistance.Count == 0) return null;
        return workPlacesOrderedByDistance[UnityEngine.Random.Range(0, workPlacesOrderedByDistance.Count / 2)].Node;
    }

    private GraphNode<SelectableObject> GetRandomStoreNode()
    {
        var storePlaces = GetStoreNodes();
        var storePlacesOrderedByDistance = storePlaces
        .Select(sp => new { Node = sp, Distance = _navigationController.GetDistance(navigationManager.GetGraphNodeForSelectableObject(gameObject.GetComponent<SelectionManager>()), sp) })
        .OrderBy(sp => sp.Distance)
        .ToList();
        if (storePlacesOrderedByDistance.Count == 0) return null;
        return storePlacesOrderedByDistance[UnityEngine.Random.Range(0, storePlacesOrderedByDistance.Count / 2)].Node;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
