using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class ResidentManager : MonoBehaviour
{
    Dictionary<Vector3Int, AbstractBuildingType> buildings;
    [SerializeField] GridManager gridManager;
    //Used for logging: private int buildingsCnt = 0;
    [SerializeField] BuildModeManager buildModeManager;
    // Start is called before the first frame update
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        buildModeManager = FindObjectOfType<BuildModeManager>();
        buildings = gridManager.GetBuildingsMap();
        //TimeManager.OnMinuteChanged += Log;
        //TimeManager.OnMinuteChanged += MoveIn;
        //TODO: IncreasePopulation called every minute after ShouldBeIncreased func finished
        TimeManager.OnHourChanged += IncreasePopulation;

    }
    public void IncreasePopulation()
    {
        //Iterate buildings
        foreach (var building in buildings)
        {
            var name = building.Value.buildingName;
            //only housing zones
            if (name.Contains("Housing Zone"))
            {
                var position = building.Key + Vector3Int.zero;
                bool nextToRoad=false;
                Dictionary<Vector3Int, AbstractBuildingType> nhbs = gridManager.GetNeigbouringBuildingsOfTile(position);
                //Debug.Log($"{building.Key} Nhbs: {nhbs.Count}");
                foreach (var nhb in nhbs)
                {
                    if (nhb.Value == null) { continue; }
                    if (nhb.Value.buildingName.Contains("Road"))
                    {
                        nextToRoad = true;
                        break;
                    }
                }
                //MoveIn only next to road zone tiles
                if (nextToRoad)
                {
                    position.y = 2; //Property 
                    var property = gridManager.GetPropertyAt(position);

                    //If there is already a property
                    if (property != null)
                    {
                        if (property.Capacity > property.HeadCount)
                        {
                            if (ShouldBeIncreased(position)) property.AddPerson();
                        }
                        else continue;
                    }
                    //If there is NO property
                    else
                    {
                        if (ShouldBeIncreased(position))
                        {
                            property = Construct(position).AddComponent<ResidentialProperty>();
                            property.AddPerson();
                            gridManager.AddProperty(position, property);
                        }
                    }
                }
            }
        }
    }

    public bool ShouldBeIncreased(Vector3Int position)
    {
        //TODO: use moral
        return true;
    }

    // Update is called once per frame
    void Update()
    { 
    }

    //Construct fun
    GameObject Construct(Vector3Int key)
    {
       return PlaceDummy(key);
    }
    GameObject PlaceDummy(Vector3Int key)
    {
        var dc = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dc.name = $"TEST Construction {(float)key.x / 2 - 5}, {(float)key.z / 2 - 5}";
        dc.transform.parent = this.transform;
        dc.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        dc.transform.localPosition = new Vector3((float)key.x / 2 - 5 - 0.25f, 0.75f, (float)key.z / 2 - 5 + 0.25f);
        return dc;
    }
    //TODO Kivenni a random fgveket ha már nincs bennük hasznosítható
    /*
    void Log()
    {
        if(buildingsCnt!=buildings.Count)
        {
            buildingsCnt = buildings.Count;
            Debug.Log("Building Count: " + buildingsCnt);
        }
        else { return; }
        int i = 1;
        string buildingsString = "";
        foreach (var building in buildings)
        {
            var name = building.Value.buildingName;
            buildingsString += $"{name} at {building.Key}\t";
            //Debug.Log("Building "+i+". name: "+name);
            if (name.Contains("Housing Zone"))
            {
                var key=building.Key+Vector3Int.zero;
                key.y = 0;
                var tile=gridManager.GetTileAtPosition(key);
                //Debug.Log($"{i}. building tile moral:{tile.tileMorality.moralityLevel}");
                /*
                if (tiles.ContainsKey(key)) Debug.Log("Van");
                else Debug.Log("nincs");
                *
            }


            i++;
        }
        Debug.Log("Buildings: " + buildingsString);
    }
    void MoveIn()
    {
        foreach (var building in buildings)
        {
            var name = building.Value.buildingName;
            //Debug.Log("Building "+i+". name: "+name);
            if (name.Contains("Housing Zone"))
            {
                var Tilepos = building.Key + Vector3Int.zero;
                Tilepos.y = 0;
                var tile = gridManager.GetTileAtPosition(Tilepos);
                //if (!tile.constructed)
                {
                    bool nextToRoad=false;
                    var key = building.Key + Vector3Int.zero;
                    //key.y = 0;
                    Dictionary<Vector3Int, AbstractBuildingType> nhbs = gridManager.GetNeigbouringBuildingsOfTile(key);
                    //Debug.Log($"{building.Key} Nhbs: {nhbs.Count}");
                    foreach(var nhb in nhbs)
                    {
                        if (nhb.Value == null) { continue; }
                        //Debug.Log(nhb.Key);
                        if(nhb.Value.buildingName.Contains("Road"))
                        {
                            nextToRoad = true;
                            break;
                        }
                    }
                    if (nextToRoad)
                    {
                        //tile.constructed=true;
                        tile.description +=" Constructed";
                        Debug.Log($"Building constucted on: {key}Tile");
                        Construct(key);
                    }
                    //Hzone.Construct();

                    /*
                    NeighbourData nhs;
                    //if(!Hzone.neighbourDatasForPositions.TryGetValue(Hzone.,nhs))
                    if(Hzone.neighbourDatasForPositions.TryGetValue(building.Key, out nhs))
                    {
                        nhs.GetNeighbourForGridPosition(building.Key);
                    }
                    *
                }
            }
        }
    }
    */

}
