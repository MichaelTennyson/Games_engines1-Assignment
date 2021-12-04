using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static void GenerateTerrainMesh(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        //for loop through height map to increment
        for (int y = 0; y < height; y++)
        {
            for (int x = 0;x < width; x++)
            {
                MeshData.vertices [vertexIndex] = new Vector3(x, heightMap[x , y], y);

                vertexIndex++;
            }
        }
    }
    
}

//public class that stores mesh data
public class MeshData
{
    //vertices array
    public Vector3[] vertices;
    public int[] triangles;

    int TriangleIndex;

    public MeshData(int MeshWidth, int MeshHeight)
    {
        vertices = new Vector3[MeshWidth * MeshHeight];
        triangles = new int[(MeshWidth - 1) * (MeshHeight - 1)*6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        //each side of the triangle is given a value so it is shaped
        triangles[TriangleIndex] = a;
        triangles[TriangleIndex + 1] = b;
        triangles[TriangleIndex + 2] = c;
        TriangleIndex += 3;
    }
}
