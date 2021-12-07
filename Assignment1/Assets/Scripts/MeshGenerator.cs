using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / -2f;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        //for loop through height map to increment
        for (int y = 0; y < height; y++)
        {
            for (int x = 0;x < width; x++)
            {
                meshData.vertices [vertexIndex] = new Vector3(topLeftX + x, heightMap[x , y] * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                if (x < width -1 && y < height - 1)
                {
                    //first triangle setup: point a, b and c are initialised
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + width);

                }


            }
            vertexIndex++;
        }
        return meshData;
    }

   
    
}

//public class that stores mesh data
public class MeshData
{
    //vertices array
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int TriangleIndex;

    public MeshData(int MeshWidth, int MeshHeight)
    {
        vertices = new Vector3[MeshWidth * MeshHeight];
        uvs = new Vector2[MeshHeight * MeshWidth];
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

    //this method 
    public Mesh createMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

}
