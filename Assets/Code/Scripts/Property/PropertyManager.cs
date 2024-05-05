using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public enum HouseLevel
{
    None,
    lvl1, lvl2, lvl3
}

public class PropertyManager : MonoBehaviour
{
    Dictionary<Vector3Int, AbstractBuildingType> buildings;
    [SerializeField] GridManager gridManager;
    //Used for logging: private int buildingsCnt = 0;
    [SerializeField] BuildModeManager buildModeManager;

    [SerializeField] List<GameObject> level1Propeties;
    [SerializeField] List<GameObject> level2Propeties;
    [SerializeField] List<GameObject> level3Propeties;

    [SerializeField] PropertyType propertyType;

    public string zoneNameSeacrhWord;

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
            //serach word in zone building name
            if (name.Contains(zoneNameSeacrhWord))
            {
                var position = building.Key + Vector3Int.zero;

                //Check if next to road
                bool nextToRoad=false;
                Dictionary<Vector3Int, AbstractBuildingType> nhbs = gridManager.GetNeigbouringBuildingsOfTile(position);
                Vector3Int nhbDir = Vector3Int.zero;
                foreach (var nhb in nhbs)
                {
                    if (nhb.Value == null) { continue; }
                    if (nhb.Value.buildingName.Contains("Road"))
                    {
                        nextToRoad = true;
                        nhbDir += nhb.Key - position;
                        Debug.Log($"nhb dir: {nhbDir.x}, {nhbDir.z}");
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
                            switch (ShouldBeIncreased(position))
                            {
                                case HouseLevel.None: break;
                                default: property.AddPerson(); break;
                            }
                               
                        }
                        else continue;
                    }
                    //If there is NO property
                    else
                    {
                        HouseLevel hlvl = ShouldBeIncreased(position);
                        switch (hlvl)
                        {
                            case HouseLevel.None:
                                break;
                            default:
                                var propertyObject = Construct(position, hlvl, nhbDir);
                                //Add script based on given property type 
                                var description = "";
                                switch (propertyType)
                                {
                                    case PropertyType.Residental:
                                        property = propertyObject.AddComponent<ResidentialProperty>();
                                        description = "Residental property";
                                        break;
                                    case PropertyType.Industrial:
                                        property = propertyObject.AddComponent<IndustrialProperty>();
                                        description = "Industrial building";
                                        break;
                                    case PropertyType.Shopping: 
                                        property= propertyObject.AddComponent<ShoppingProperty>();
                                        description = "Shopping building";
                                        break;
                                }
                                property.PropertyGameObject = propertyObject;
                                var selectionManager = propertyObject.AddComponent<SelectionManager>();
                                selectionManager.Init(position, description, SelectableObjectType.ZoneBuilding);
                                var highlight = propertyObject.AddComponent<Highlight>();
                                highlight.SetRenderers(new List<Renderer>{propertyObject.GetComponent<Renderer>()});
                                highlight.SetHighlightColor(Color.white);
                                property.AddPerson();
                                gridManager.AddProperty(position, property);
                                break;
                        }
                    }
                }
            }
        }
    }

    public HouseLevel ShouldBeIncreased(Vector3Int position)
    {
        //TODO: use moral
        Vector3Int tilePos = position+Vector3Int.zero;
        tilePos.y = 0;
        Tile tile = gridManager.GetTileAtPosition(tilePos);
        float moral=tile.tileMorality.moralityLevel;
        HouseLevel houselevel=HouseLevel.None;

        //moral increase with random

        if (moral < 2) houselevel = HouseLevel.lvl1;

        else if(2 <= moral && moral < 3) houselevel= HouseLevel.lvl2;

        else if(3 <= moral) houselevel = HouseLevel.lvl3;

        return houselevel;
    }

    // Update is called once per frame
    void Update()
    { 
    }

    //Construct fun
    GameObject Construct(Vector3Int key,HouseLevel houselvl, Vector3Int roadDir)
    {
        GameObject house=null;
        int random;
        switch (houselvl)
        {
            case HouseLevel.lvl1:
                random = Random.Range(0, level1Propeties.Count);
                house = level1Propeties[random];
                break;
            case HouseLevel.lvl2:
                random = Random.Range(0, level2Propeties.Count);
                house = level2Propeties[random];
                break;
            case HouseLevel.lvl3:
                random = Random.Range(0, level3Propeties.Count);
                house = level3Propeties[random];
                break;
        }




        return PlaceBuilding(key, house,roadDir);
       //return PlaceDummy(key);
    }
    GameObject PlaceBuilding(Vector3Int key,GameObject prefab, Vector3Int roadDir)
    {
        var dc = Instantiate(prefab);

        dc.name = $"{propertyType.ToString()} Property  {(float)key.x / 2 - 5}, {(float)key.z / 2 - 5}";
        dc.transform.parent = this.transform;
        dc.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        dc.transform.localPosition = new Vector3((float)key.x / 2 - 5 - 0.25f, 0.5f, (float)key.z / 2 - 5 + 0.25f);
        // 1,0,0 rotate right 90
        if (roadDir.x == 1)
        {
            dc.transform.Rotate(new Vector3(0, 1, 0), 90);
        }
        // -1,0,0 rotate left 90
        else if (roadDir.x == -1)
        {
            dc.transform.Rotate(new Vector3(0, 1, 0), -90);
        }
        //rotate 180 if 0,0,-1
        else if(roadDir.z == -1)
        {
            dc.transform.Rotate(new Vector3(0, 1, 0), 180);
        }

        //if 0,0,1 NO rotate
        return dc;
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
    //TODO Kivenni a random fgveket ha m�r nincs benn�k hasznos�that�
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
                var tilePos=gridManager.GetTileAtPosition(key);
                //Debug.Log($"{i}. building tilePos moral:{tilePos.tileMorality.moralityLevel}");
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
                var tilePos = gridManager.GetTileAtPosition(Tilepos);
                //if (!tilePos.constructed)
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
                        //tilePos.constructed=true;
                        tilePos.description +=" Constructed";
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
