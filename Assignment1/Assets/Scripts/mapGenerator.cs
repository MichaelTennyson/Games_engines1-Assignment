using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


public class mapGenerator : MonoBehaviour
{
    public enum DrawMode{NoiseMap, ColourMap, Mesh}
    public DrawMode drawMode;

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int EditorPreviewLOD;
    public float noiseScale;

    public bool autoUpdate;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;
    public TerrainType[] regions;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    public void DrawMapEditor() 
    {
        MapData mapData = generateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            //method call fot the Drawtexture method passing the noisemap
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            //method call fot the Drawtexture method passing colour map
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            //method call fot the Drawtexture method passing mesh map
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, EditorPreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
    }

    public void RequestMapData(Vector2 centre,Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback);

        };

        new Thread(threadStart).Start();

    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = generateMapData(centre);
        //this initialisation is locked because it could be possibly accessed by other threads that are running at the same time
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData,int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();

    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback (threadInfo.parameter);
                   

            }
        }
    }
    MapData generateMapData(Vector2 centre)
    {
        float[,] noiseMap = noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for(int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int  i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                        break; 
                    }
                }
            }
        }

        return new MapData(noiseMap, colourMap);
    }


    private void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

    }

    struct MapThreadInfo<T>
    {
        public Action<T> callback;
        public T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;

        }

    } 

}

[System.Serializable]

//terain type structure
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}


//map data structure
public struct MapData
{
    public float[,] heightMap;
    public Color[] colourMap;
    
    public MapData (float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;

    }
}
