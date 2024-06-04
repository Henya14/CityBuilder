using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResidentialProperty : AbstarctProperty
{
    float chanceOfSpawningCar = 0.1f;
    NavigationManager navigationManager;
    // Start is called before the first frame update
    void Start()
    {
        this.PropertyType = PropertyType.Residental;
        navigationManager = FindObjectOfType<NavigationManager>();
        TimeManager.OnMinuteChanged += CarSpawn;
    }

    private void CarSpawn()
    {
        var rand = UnityEngine.Random.Range(0.0f, 1.0f);

        if (rand < chanceOfSpawningCar) 
        {
            var start = navigationManager.GetGraphNodeForSelectableObject(gameObject.GetComponent<SelectionManager>());
            var type = UnityEngine.Random.Range(1, 3);
            List<GraphNode<SelectableObject>> destinations = new List<GraphNode<SelectableObject>>(); 
            switch (type) {
                case 1:
                    destinations = GetStoreNodes();
                    break;
                case 2:
                    destinations = GetBuildingNodes();
                    break;
            }
            List<GraphSearchNode<SelectableObject>> route = null;
            destinations =  destinations.OrderBy(_ => Guid.NewGuid()).ToList();
            foreach(var dest in destinations) 
            {
                navigationManager.FindShortestPathBeetweenTwoPoints(start, dest, out route);
                if (route != null) {
                    break;
                }
            }
            if (route != null) {
                navigationManager.StartCarOnRoute(route);
            }
        }
    }

    List<GraphNode<SelectableObject>> GetStoreNodes() 
    {
        var stores = navigationManager.WhereBuildings(gn => gn.Value.GetSelectableObjectType() == SelectableObjectType.ZoneBuilding && gn.Value.GetDescription().Contains("Shopping"));
        return stores;
    }

    List<GraphNode<SelectableObject>> GetBuildingNodes() 
    {
        var buildings = navigationManager.WhereBuildings(gn => gn.Value.GetSelectableObjectType() == SelectableObjectType.Building);
        return buildings;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
