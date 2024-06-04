using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenarator : MonoBehaviour
{
   [SerializeField] int mapWidth;
   [SerializeField] int mapHeight;
   [SerializeField] float noiseScale;
   [SerializeField] public bool autoUpdateEnabled;

   public void GenerateMap() 
   {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);


        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        mapDisplay.DrawNoiseMap(noiseMap);
   }

}
