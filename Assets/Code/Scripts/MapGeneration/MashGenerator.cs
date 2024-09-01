using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class MashGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve meshHeightCurve)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftXCoordinate = -(width - 1) / 2f;
        float topLeftZCoordinate = (height - 1) / 2f;
        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float vertexYCoordinate = meshHeightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                meshData.vertices[vertexIndex] = new Vector3(topLeftXCoordinate + x, vertexYCoordinate, topLeftZCoordinate - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                if (x < width - 1 && y < height - 1)
                {
                    /*
                        A __ D
                         |__|
                        B    C
                    */
                    int a = vertexIndex;
                    int b = vertexIndex + width;
                    int c = vertexIndex + width + 1;
                    int d = vertexIndex + 1;
                    meshData.AddTriangle(a, c, b);
                    meshData.AddTriangle(c, a, d);
                }
                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = this.vertices,
            triangles = this.triangles,
            uv = this.uvs
        };

        mesh.RecalculateNormals();
        return mesh;
    }
}