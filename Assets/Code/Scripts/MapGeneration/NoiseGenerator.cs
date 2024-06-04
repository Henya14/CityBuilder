using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator 
{
    public static float[,] GenerateNoiseMap(int width, int height, float scale) {
        float[,] heightMap = new float[width, height];
        
        if (scale <= 0.0f) {
            scale = 0.0001f;
        }
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++)
            {
                float noiseSampleCoordinateX = x / scale;
                float noiseSampleCoordinateY = y / scale;

                float noiseValue = Mathf.PerlinNoise(noiseSampleCoordinateX, noiseSampleCoordinateY);
                heightMap[x,y] = noiseValue;
            }
        }
        
        return heightMap;
    }
}
