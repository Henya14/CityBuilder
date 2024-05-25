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
    //TODO: Move it to Function
    [SerializeField] SerializableList<TileSaveData> tilesToSave;
    [SerializeField] SerializableList<BuildingSaveData> buildingsToSave;

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
        tilesToSave = serializableList;
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
            tilesToSave.list.Add(data);
        }
        string tileJson = JsonUtility.ToJson(tilesToSave);
        /*
        Debug.Log(tileJson);
        */
        File.WriteAllText(saveFileName + "Tiles.json", tileJson);

    }
    public void LoadTiles()
    {
        ResetTilesList();

        string tileJson = File.ReadAllText(saveFileName + "Tiles.json");
        tilesToSave = JsonUtility.FromJson<SerializableList<TileSaveData>>(tileJson);

        /*
        foreach (var TSD in tilesToSave.list)
        {
            Debug.Log(TSD.PositionX+" "+ TSD.Name);
        }
        */

        gridManager.LoadGrid(tilesToSave.list);
    }
    private void ResetBuildingsList()
    {
        SerializableList<BuildingSaveData> serializableList = new SerializableList<BuildingSaveData>();
        serializableList.list = new List<BuildingSaveData>();
        buildingsToSave = serializableList;
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

            buildingsToSave.list.Add(data);
        }
        string buildingJson = JsonUtility.ToJson(buildingsToSave);
        /*
        Debug.Log(buildingJson);
        */
        File.WriteAllText(saveFileName+"Buildings.json", buildingJson);

    }
    public void LoadBuildings()
    {
        ResetBuildingsList();

        string buildingJson = File.ReadAllText(saveFileName + "Buildings.json");
        buildingsToSave = JsonUtility.FromJson<SerializableList<BuildingSaveData>>(buildingJson);

        buildModeManager.LoadBuildings(buildingsToSave.list);
    }

    public void SaveProperties(Dictionary<Vector3Int, AbstarctProperty> propertyMap)
    {

    }

    public void LoadProperties()
    {

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
    public int PositionX;
    public int PositionY;
    public int PositionZ;
    //Property:
    public int Capacity;
    public int HeadCount;
    public PropertyType PropertyType;
    public int MaxCapacity;
    //SelectionManager:
    public string Description;

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
}

[Serializable]
public class SerializableList<T>
{
    public List<T> list;
}