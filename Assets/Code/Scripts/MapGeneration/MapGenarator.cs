using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapGenarator : MonoBehaviour
{
   public enum DrawMode { NoiseMap, ColorMap, Mesh };
   public DrawMode drawMode;
   [SerializeField] int mapWidth;
   [SerializeField] int mapHeight;
   [SerializeField] int seed;
   [SerializeField] float noiseScale;
   [SerializeField] int iterations;
   [Range(0,1)]
   [SerializeField] float amplitudeChangeFactor;
   [SerializeField] float frequencyChangeFactor;
   [SerializeField] Vector2 noiseOffset;
   [SerializeField] public bool autoUpdateEnabled;
   public TerrainType[] terrainTypes;
   public float meshHeightMultiplier;
   public AnimationCurve meshHeightCurve;

   public void GenerateMap()
   {
      float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, iterations, amplitudeChangeFactor, frequencyChangeFactor, noiseOffset);

      Color[] colorMap = new Color[mapWidth * mapHeight];
      float maxHeight = 0;

      for (int y = 0; y < mapHeight; y++)
      {
         for (int x = 0; x < mapWidth; x++)
         {
            float height = noiseMap[x, y];

            if (height > maxHeight){
               maxHeight = height;
            }
            for (int i = 0; i < terrainTypes.Length; i++)
            {
               if (height <= terrainTypes[i].height)
               {
                  TerrainType terrainType = terrainTypes[i];
                  colorMap[y * mapWidth + x] = terrainType.color;
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
          mapDisplay.DrawTexture(TextureGenerator.CreateTextureFromColorMap(colorMap, mapWidth, mapHeight));
      }  else if (drawMode == DrawMode.Mesh)
      {
          mapDisplay.DrawMesh(MashGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve), TextureGenerator.CreateTextureFromColorMap(colorMap, mapWidth, mapHeight));
      }
   }

   void OnValidate()
   {
      if (mapWidth < 1)
      {
         mapWidth = 1;
      }

      if (mapHeight < 1)
      {
         mapHeight = 1;
      }

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
