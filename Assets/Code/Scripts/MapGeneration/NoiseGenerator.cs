using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    const int RANDOM_OFFSET_MAX_VALUE = 10;
    const int RANDOM_OFFSET_MIN_VALUE = -10;
    public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int iterations, float amplitudeChangeFactor, float frequencyChangeFactor, Vector2 noiseOffset)
    {
        float[,] noiseMap = new float[width, height];
        System.Random randomGenerator = new System.Random(seed);

        Vector2[] randomOffsetForIterations = new Vector2[iterations];

        for (int i = 0; i < iterations; i++) {
            float randomOffsetX = randomGenerator.Next(RANDOM_OFFSET_MIN_VALUE, RANDOM_OFFSET_MAX_VALUE) + noiseOffset.x;
            float randomOffsetY = randomGenerator.Next(RANDOM_OFFSET_MIN_VALUE, RANDOM_OFFSET_MAX_VALUE) + noiseOffset.y;
            randomOffsetForIterations[i] = new Vector2(randomOffsetX, randomOffsetY);
        }
        if (scale <= 0.0f)
        {
            scale = 0.0001f;
        }

        float minNoiseValue = float.MaxValue;
        float maxNoiseValue = float.MinValue;
        float halfWidth = width / 2;
        float halfHeight = height / 2;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                noiseMap[x, y] += 0.0f;
                for (int i = 0; i < iterations; i++)
                {
                    float noiseSampleCoordinateX = (x - halfWidth) / scale * frequency + randomOffsetForIterations[i].x;
                    float noiseSampleCoordinateY = (y - halfHeight) / scale * frequency + randomOffsetForIterations[i].y;
                    float noiseValue = 2 * Mathf.PerlinNoise(noiseSampleCoordinateX, noiseSampleCoordinateY) - 1.0f;
                    noiseValue *= amplitude;
                    noiseMap[x, y] += noiseValue;

                    if (noiseValue < minNoiseValue)
                    {
                        minNoiseValue = noiseValue;
                    }
                    if (noiseValue > maxNoiseValue)
                    {
                        maxNoiseValue = noiseValue;
                    }
                    amplitude *= amplitudeChangeFactor;
                    frequency *= frequencyChangeFactor;
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}
