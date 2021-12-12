using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

//this class will contain methods that generate an infinite number of plain tiles
public class EndlessTerrain : MonoBehaviour
{
    public static float maxViewDst;
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewPosition;
    int chunkSize;
    int chunksVisibleInViewDst;
    static mapGenerator mapGenerator;
    public LODinfo[] detailLevels;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    //list varibale that stores a list of the past terrainChunk values
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

     void Start()
    {
        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;

        mapGenerator = FindObjectOfType<mapGenerator>();
        chunkSize = mapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
    }

    void Update()
    {
        viewPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        //for loop iterates through the number of values in the list
        for(int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }

        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewPosition.y / chunkSize);
        
        //in this for loop 
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
            {
                // the chunk coordinates are set to the sum of the current chunk coord and the offset
                Vector2 viewChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewChunkCoord))
                {
                    terrainChunkDictionary[viewChunkCoord].UpdateTerrainChunk();
                    //if tile is visible on the screen. the terrainChunksvisiblelastUpdatelist is updated
                    if (terrainChunkDictionary[viewChunkCoord].isVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewChunkCoord, new TerrainChunk(viewChunkCoord, chunkSize,detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;


        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODinfo[] detailLevels;
        LODMesh[] LODMeshes;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODinfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;

            position = coord * size;
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            Bounds bounds = new Bounds(position, Vector2.one * size);
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            meshRenderer.material = material;
            LODMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                LODMeshes[i] = new LODMesh(detailLevels[i].lod);
            }
       
            SetVisible(false);

            mapGenerator.RequestMapData(onMapDataRecieved);

        }

        void onMapDataRecieved(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;
                

        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            meshFilter.mesh = meshData.createMesh();
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewPosition));
                bool visible = viewDstFromNearestEdge <= maxViewDst;
                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = LODMeshes[lodIndex];
                        if (lodMesh.HasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }
                SetVisible(visible);
            } 
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool isVisible()
        {
            return meshObject.activeSelf;
        }

    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool HasMesh;
        int lod;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            mesh = meshData.createMesh();
            HasMesh = true;
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
        }
    }

    [System.Serializable]
    public struct LODinfo
    {
        public int lod;
        public float visibleDstThreshold;
    }
}
