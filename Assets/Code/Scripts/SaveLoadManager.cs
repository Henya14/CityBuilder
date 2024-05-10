using Palmmedia.ReportGenerator.Core.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{

    [SerializeField] BuildModeManager buildModeManager;
    [SerializeField] GridManager gridManager;
    [SerializeField] string saveFileName;
    bool saved=false;

    [SerializeField] SerializableList<TileData> tilesToSave;
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
        /*
        if (!saved)
        {
            gridManager.Save();
            saved = true;
        }
        else
        {
            gridManager.Load();
        }
        */
    }
   public void SaveTiles(Dictionary<Vector3Int, Tile> tileMap)
    {

        foreach (var VandB in tileMap)
        {
            TileData data = new TileData()
            {
                PositionX = VandB.Key.x,
                PositionY = VandB.Key.y,
                PositionZ = VandB.Key.z
            };
            data.ConvertTile(VandB.Value);
            tilesToSave.list.Add(data);
        }
        string tileJson = JsonUtility.ToJson(tilesToSave);
        Debug.Log(tileJson);
        File.WriteAllText(saveFileName + "Tiles.json", tileJson);

    }
    public void LoadTiles()
    {
        string tileJson = File.ReadAllText(saveFileName + "Tiles.json");
        tilesToSave = JsonUtility.FromJson<SerializableList<TileData>>(tileJson);


        foreach (var TD in tilesToSave.list)
        {
            Debug.Log(TD.PositionX+" "+ TD.Name);
        }


        gridManager.LoadGrid(tilesToSave.list);
    }

    public void SaveBuildings(Dictionary<Vector3Int, AbstractBuildingType> buildingsMap)
    {

    }
    public void LoadBuildings()
    {

    }

    public void SaveProperties(Dictionary<Vector3Int, AbstarctProperty> propertyMap)
    {

    }

    public void LoadProperties()
    {

    }

}
[Serializable]
public class TileData
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
        ConvertMorality(tile.tileMorality);
    }
    
    public void ConvertMorality(Morality morality) 
    {
        moralityLevel = morality.moralityLevel;
    }

}

[Serializable]
public class SerializableList<T>
{
    public List<T> list;
}