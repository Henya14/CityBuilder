using JetBrains.Annotations;
using Palmmedia.ReportGenerator.Core.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Video;

public class SaveLoadManager : MonoBehaviour
{

    [SerializeField] BuildModeManager buildModeManager;
    [SerializeField] GridManager gridManager;
    [SerializeField] string saveFileName;

    //used for testing
    [SerializeField] bool saved=true;
    [SerializeField] bool TurnedOff=true;
    //Suggestion: Move it to Function
    [SerializeField] SerializableList<TileSaveData> tilesToSaveOrLoad;
    [SerializeField] SerializableList<BuildingSaveData> buildingsToSaveOrLoad;
    [SerializeField] SerializableList<PropertySaveData> propertyToSaveOrLoad;
    [SerializeField] PlayerSaveData playerSaveData;

    // Start is called before the first frame update
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        buildModeManager = FindObjectOfType<BuildModeManager>();

        TimeManager.OnHourChanged += DummySaveLoad;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void DummySaveLoad()
    {
        if (TurnedOff) return;
        if (!saved)
        {
            gridManager.Save();
            //saved = true;
        }
        else
        {
            gridManager.Load();
        }
        
    }
    private void ResetTilesList()
    {
        SerializableList<TileSaveData> serializableList = new SerializableList<TileSaveData>();
        serializableList.list = new List<TileSaveData>();
        tilesToSaveOrLoad = serializableList;
    }
    public void SaveTiles(Dictionary<Vector3Int, Tile> tileMap)
    {
        ResetTilesList();

        foreach (var VandB in tileMap)
        {
            TileSaveData data = new TileSaveData()
            {
                PositionX = VandB.Key.x,
                PositionY = VandB.Key.y,
                PositionZ = VandB.Key.z
            };
            data.ConvertTile(VandB.Value);
            tilesToSaveOrLoad.list.Add(data);
        }
        string tileJson = JsonUtility.ToJson(tilesToSaveOrLoad);
        /*
        Debug.Log(tileJson);
        */
        File.WriteAllText(saveFileName + "Tiles.json", tileJson);

    }
    public void LoadTiles()
    {
        ResetTilesList();

        string tileJson = File.ReadAllText(saveFileName + "Tiles.json");
        tilesToSaveOrLoad = JsonUtility.FromJson<SerializableList<TileSaveData>>(tileJson);

        /*
        foreach (var TSD in tilesToSaveOrLoad.list)
        {
            Debug.Log(TSD.PositionX+" "+ TSD.Name);
        }
        */

        gridManager.LoadGrid(tilesToSaveOrLoad.list);
    }
    private void ResetBuildingsList()
    {
        SerializableList<BuildingSaveData> serializableList = new SerializableList<BuildingSaveData>();
        serializableList.list = new List<BuildingSaveData>();
        buildingsToSaveOrLoad = serializableList;
    }
    public void SaveBuildings(Dictionary<Vector3Int, AbstractBuildingType> buildingsMap)
    {
        ResetBuildingsList();

        //For dont duplicat big buildings
        List<Vector3Int> savedList = new List<Vector3Int>();
        foreach (var VandB in buildingsMap)
        {
            if (savedList.Contains(VandB.Value.gridPositions[0])) continue;

            savedList.Add(VandB.Value.gridPositions[0]);

            BuildingSaveData data = new BuildingSaveData();

            SerializableList<SVector3> list = new SerializableList<SVector3>();
            List<SVector3> sVector3s = new List<SVector3>();
            list.list=sVector3s;
            data.GridPositions= list;

            foreach(var gridpos in VandB.Value.gridPositions)
            {
                
                data.GridPositions.list.Add(new SVector3(gridpos));
            }
            

            data.ConvertBuildingData(VandB.Value.GetBuildingData());

            buildingsToSaveOrLoad.list.Add(data);
        }
        string buildingJson = JsonUtility.ToJson(buildingsToSaveOrLoad);
        /*
        Debug.Log(buildingJson);
        */
        File.WriteAllText(saveFileName+"Buildings.json", buildingJson);

    }
    public void LoadBuildings()
    {
        ResetBuildingsList();

        string buildingJson = File.ReadAllText(saveFileName + "Buildings.json");
        buildingsToSaveOrLoad = JsonUtility.FromJson<SerializableList<BuildingSaveData>>(buildingJson);

        buildModeManager.LoadBuildings(buildingsToSaveOrLoad.list);
    }

    private void ResetPropertiesList()
    {
        SerializableList<PropertySaveData> serializableList = new SerializableList<PropertySaveData>();
        serializableList.list = new List<PropertySaveData>();
        propertyToSaveOrLoad = serializableList;
    }
    public void SaveProperties(Dictionary<Vector3Int, AbstarctProperty> propertyMap)
    {
        ResetPropertiesList();

        foreach(var property in propertyMap)
        {
            PropertySaveData propertySaveData = new PropertySaveData();
            propertySaveData.Convert(property.Value);
            propertySaveData.sVector3 = new SVector3(property.Key);

            propertyToSaveOrLoad.list.Add(propertySaveData);
        }
        string propertyJson = JsonUtility.ToJson(propertyToSaveOrLoad);

        File.WriteAllText(saveFileName + "Properties.json", propertyJson);

    }

    public void LoadProperties()
    {
        ResetPropertiesList();

        string propertyJson = File.ReadAllText(saveFileName + "Properties.json");
        propertyToSaveOrLoad = JsonUtility.FromJson<SerializableList<PropertySaveData>>(propertyJson);

        foreach(var manager in FindObjectsOfType<PropertyManager>())
        {
            manager.loadProperties(propertyToSaveOrLoad.list);
        }

    }

    public void ResetRest() => playerSaveData=new PlayerSaveData();
    public void SaveRest()
    {
        ResetRest();
        playerSaveData.Hour = TimeManager.Hour;
        playerSaveData.Minute = TimeManager.Minute;

        playerSaveData.Balance = PlayerBalance.Balance;
        playerSaveData.Coal=PlayerBalance.Coal;
        playerSaveData.Eletricty=PlayerBalance.Electricity;
        playerSaveData.Wood = PlayerBalance.Wood;

        playerSaveData.RresidentsTaxes = new SerializableList<float>();
        playerSaveData.RresidentsTaxes.list = PlayerBalance.instance.GetResidnetTaxes();
        playerSaveData.ShopTaxes = new SerializableList<float>();
        playerSaveData.ShopTaxes.list = PlayerBalance.instance.GetShopTaxes();
        playerSaveData.FactoryTaxes = new SerializableList<float>();
        playerSaveData.FactoryTaxes.list = PlayerBalance.instance.GetFactoryTaxes();


        string playerJson = JsonUtility.ToJson(playerSaveData);
        File.WriteAllText(saveFileName + "Player.json", playerJson);

    }

    public void LoadRest()
    {
        ResetRest();
        TimeManager.instance.StartStopTimer();

        string playerJson = File.ReadAllText(saveFileName + "Player.json");
        playerSaveData = JsonUtility.FromJson<PlayerSaveData>(playerJson);

        TimeManager.instance.LoadTime(playerSaveData.Hour, playerSaveData.Minute);

        PlayerBalance.instance.LoadData(playerSaveData);

        GameUIManager guiM = FindObjectOfType<GameUIManager>();
        guiM.UpdateTimer();
        guiM.updateBalanceText();
    }
}
[Serializable]
public class TileSaveData
{
    //Vector3Int:
    public int PositionX;
    public int PositionY;
    public int PositionZ;
    //Tile:
    public string Name;
    public string Description;
    public float moralityLevel;

    public void ConvertTile(Tile tile)
    {
        Name = tile.Name;
        Description = tile.Description;
        moralityLevel=tile.tileMorality.moralityLevel;
    }
    

}

[Serializable]
public class BuildingSaveData
{
    //Vector3Int:
    public SerializableList<SVector3> GridPositions;
    //Building Data
    public string Name;
    public string Description;
    public BuildingType BuildingType;
    public int Price;
    public int SizeX;
    public int SizeY;
    //public bool IsAvailable;


    public void ConvertBuildingData(BuildingData buildingData)
    {
        Name= buildingData.name;
        Description=buildingData.Description;
        BuildingType=buildingData.buildingType;
        Price=buildingData.price;
        SizeX = buildingData.size.x;
        SizeY = buildingData.size.y;

    }
    public Dictionary<Vector3, List<Vector3Int>> GetDictionary(GridManager gridManager)
    {
        List<Vector3Int> list = new List<Vector3Int>();
        foreach (var sv3 in GridPositions.list)
        {
            list.Add(new Vector3Int(sv3.X, 0, sv3.Z));
        }
        Dictionary<Vector3, List<Vector3Int>> placingPositionsWithGridPositions = new Dictionary<Vector3, List<Vector3Int>>();
        if (BuildingType == BuildingType.IndividualBuilding)
        {
            placingPositionsWithGridPositions[gridManager.GetSelectionCenter(list)]= list;
        }

        else
        {
            list.ForEach(item =>
            {
                var selectionCenter = gridManager.GetSelectionCenter(new List<Vector3Int> { item });
                placingPositionsWithGridPositions[selectionCenter] = new List<Vector3Int> { item };
            });
        }
        return placingPositionsWithGridPositions;
    }
    public BuildingData GetBuildingData()
    {
        BuildingData buildingData = ScriptableObject.CreateInstance<BuildingData>();
        buildingData.Name = Name;
        buildingData.Description = Description;
        buildingData.buildingType = BuildingType;
        buildingData.price = Price;
        buildingData.size= new Vector2Int(SizeX,SizeY);

        //GetPrefab
        BuildingData[] loadedObjects = Resources.LoadAll<BuildingData>("Buildings");
        
        foreach (BuildingData obj in loadedObjects)
        {
            if (obj.Description == Description)
            {
                buildingData.prefab=obj.prefab;
            }
        }
        Debug.Log(buildingData.prefab == null ? "Cant find prefab" : "Prefab found");
        return buildingData;
    }
}
[Serializable]
public class PropertySaveData
{
    //Vector3Int:
    public SVector3 sVector3;
    //Property:
    public int Capacity;
    public int HeadCount;
    public PropertyType PropertyType;
    public int MaxCapacity;
    public HouseLevel HouseLevel;
    
    public void Convert(AbstarctProperty property)
    {
        Capacity= property.Capacity;
        HeadCount= property.HeadCount;
        PropertyType= property.PropertyType;
        MaxCapacity= property.MaxCapacity;
        HouseLevel= property.HouseLevel;
    }

}
[Serializable]
public class SVector3
{
    public int X, Y, Z;

    public SVector3() { X = 0; Y = 0; Z = 0; }
    public SVector3(Vector3Int vector3Int)
    {
        Convert(vector3Int);
    }
    public void Convert(Vector3Int v)
    {
        X=v.x; Y=v.y; Z=v.z;
    }
    public string ToStrig()
    {
        return $"X: {X}, Y: {Y}, Z: {Z}";
    }
    public Vector3Int ConvertBack() => new Vector3Int(X,Y,Z);
}

[Serializable]
public class PlayerSaveData
{
    //Time:
    public int Hour;
    public int Minute;
    //PlayerBalance:
    public int Balance;
    public int Wood;
    public int Eletricty;
    public int Coal;
    public SerializableList<float> RresidentsTaxes;
    public SerializableList<float> ShopTaxes;
    public SerializableList<float> FactoryTaxes;
    //TODO: Popolation ?

}

[Serializable]
public class SerializableList<T>
{
    public List<T> list;
}