using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapGenarator : MonoBehaviour
{
   public enum DrawMode { NoiseMap, ColorMap, Mesh };
   public DrawMode drawMode;
   const int mapChunkSize = 241;
   [Range(0, 6)]
   public int levelOfDetail;
   [SerializeField] int seed;
   [SerializeField] float noiseScale;
   [SerializeField] int iterations;
   [Range(0, 1)]
   [SerializeField] float amplitudeChangeFactor;
   [SerializeField] float frequencyChangeFactor;
   [SerializeField] Vector2 noiseOffset;
   [SerializeField] public bool autoUpdateEnabled;
   public TerrainType[] terrainTypes;
   public float meshHeightMultiplier;
   public AnimationCurve meshHeightCurve;

   public TreeManager treeManager;

   void Start()
   {
      treeManager = FindObjectOfType<TreeManager>();
      GenerateMap();

   }
   public void GenerateMap()
   {
      float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, iterations, amplitudeChangeFactor, frequencyChangeFactor, noiseOffset);

      Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
      float maxHeight = 0;

      for (int y = 0; y < mapChunkSize; y++)
      {
         for (int x = 0; x < mapChunkSize; x++)
         {
            float height = noiseMap[x, y];

            if (height > maxHeight)
            {
               maxHeight = height;
            }
            for (int i = 0; i < terrainTypes.Length; i++)
            {
               if (height <= terrainTypes[i].height)
               {
                  TerrainType terrainType = terrainTypes[i];
                  colorMap[y * mapChunkSize + x] = terrainType.color;
                  //colorMap[y * mapChunkSize + x] = Color.Lerp(Color.black, Color.white, height);
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
      }
      else if (drawMode == DrawMode.Mesh)
      {
         mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.CreateTextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
         if (treeManager != null)
         {
            treeManager.Init();
            treeManager.GenerateTrees();
         }
      }
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
