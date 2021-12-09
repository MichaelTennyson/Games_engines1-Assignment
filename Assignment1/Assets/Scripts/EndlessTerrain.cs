using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

//this class will contain methods that generate an infinite number of plain tiles
public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDst = 450;
    public Transform viewer;

    public static Vector2 viewPosition;
    int chunkSize;
    int chunksVisibleInViewDst;
    static mapGenerator mapGenerator;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    //list varibale that stores a list of the past terrainChunk values
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

     void Start()
    {
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
                    terrainChunkDictionary.Add(viewChunkCoord, new TerrainChunk(viewChunkCoord, chunkSize, transform));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            Bounds bounds = new Bounds(position, Vector2.one * size);
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * size /10f;
            SetVisible(false);

            mapGenerator.RequestMapData(onMapDataRecieved);

        }

        void onMapDataRecieved(MapData mapData)
        {
            print("map data Recieved");

        }

        public void UpdateTerrainChunk()
        {
          float viewDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewPosition));
            bool visible = viewDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);
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
}
