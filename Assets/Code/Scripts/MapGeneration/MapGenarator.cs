using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGenarator : MonoBehaviour
{
   public enum DrawMode { NoiseMap, ColorMap, Mesh };
   public DrawMode drawMode;
   const int mapChunkSize = 241;
   [Range(0,6)]
   public int levelOfDetail;
   [SerializeField] int seed;
   [SerializeField] float noiseScale;
   [SerializeField] int iterations;
   [Range(0,1)]
   [SerializeField] float amplitudeChangeFactor;
   [SerializeField] float frequencyChangeFactor;
   [SerializeField] Vector2 noiseOffset;
   [SerializeField] public bool autoUpdateEnabled;
   public TerrainType[] terrainTypes;

   [SerializeField] RawMaterialManager materialManager;
   [Range(0, 1)]
   [SerializeField] float rawMaterialLowestAltitude;
   [Range(0, 1)]
   [SerializeField] float rawMaterialHighestAltitude;
   public float meshHeightMultiplier;
   public AnimationCurve meshHeightCurve;

   void Start() {
        
        GenerateMap();
   }
   public void GenerateMap()
   {
      if(rawMaterialLowestAltitude > rawMaterialHighestAltitude)
      {
          rawMaterialLowestAltitude = 0;
          rawMaterialHighestAltitude = 1;
      }
      materialManager.LoadRawMaterials();
      List<RawMaterialWithRearity> rawMaterials = new(materialManager.GetRawMaterials());

      float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, iterations, amplitudeChangeFactor, frequencyChangeFactor, noiseOffset);

      Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
      float maxHeight = 0;

      for (int y = 0; y < mapChunkSize; y++)
      {
         for (int x = 0; x < mapChunkSize; x++)
         {
            float height = noiseMap[x, y];

            if (height > maxHeight){
               maxHeight = height;
            }
            //Raw Material Generation
            bool thereIsMaterial=false;
            if(rawMaterialLowestAltitude <= height && height <= rawMaterialHighestAltitude)
            foreach(var rm in rawMaterials)
            {
                var temp = y * mapChunkSize + x;
                if(0==(temp%(int)(rm.Rearity*1000))){
                    colorMap[y * mapChunkSize + x] = rm.Color;
                    thereIsMaterial = true;
                    //Debug.Log($"-----------{y * mapChunkSize + x}, {rm.Type}");
                    materialManager.AddRect(RawMaterialPlaced(x, y));
                    break;
                }
            }
            if (thereIsMaterial) { continue; }


            for (int i = 0; i < terrainTypes.Length; i++)
            {
               if (height <= terrainTypes[i].height)
               {
                  TerrainType terrainType = terrainTypes[i];
                  colorMap[y * mapChunkSize + x] = terrainType.color;
                  break;
               }
            }


         }
      }
      MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
      if (drawMode == DrawMode.NoiseMap)
      {
         mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
      }
      else if (drawMode == DrawMode.ColorMap)
      {
          mapDisplay.DrawTexture(TextureGenerator.CreateTextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
      }  else if (drawMode == DrawMode.Mesh)
      {
          mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.CreateTextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
      }
   }
    private Rect RawMaterialPlaced(int x, int y) //-1200,0,1200 -> 1200,0,-1200 , +10x, #, -10y, 
    {
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();

        GameObject mesh = mapDisplay.meshRenderer.gameObject;
        float xScale = 10;
        float yScale = 10;
        float xCorner = (mapChunkSize - 1) / -2;
        float yCorner = (mapChunkSize - 1) / 2;
        if (mesh != null)
        {
            xScale = mesh.transform.localScale.x;
            yScale = mesh.transform.localScale.z;
        }
        else
        {
            Debug.Log("Mesh not found");
        }

        //Rect rect = new Rect(-1200+(10*x),1200-10*(y+1),10,10);
        Rect rect = new Rect(xScale*(xCorner +  x), yScale*(yCorner - (y + 1)), xScale, yScale);
        /*
        GameObject recto = GameObject.CreatePrimitive(PrimitiveType.Cube);
        recto.transform.localPosition = new Vector3(rect.x+ xScale/2, 2, rect.y+ yScale/2);
        recto.transform.localScale = new Vector3(xScale, 1, yScale);
        if(rect.x==1200 || rect.y == -1200)
        {
            Debug.LogWarning($"x: {x}, y: {y}");
        }
        */
        return rect;
    }


   void OnValidate()
   {
      if (amplitudeChangeFactor < 0.0f)
      {
         amplitudeChangeFactor = 0.0f;
      }

      if (frequencyChangeFactor < 0.0f)
      {
         frequencyChangeFactor = 0.0f;
      }

      if (iterations < 0)
      {
         iterations = 0;
      }
   }
}

[System.Serializable]
public struct TerrainType
{
   public string name;
   public float height;
   public Color color;

}
