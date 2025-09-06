using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    public int worldSizeRadius = 16;
    public int seed;
    
    public GameObject chunkPrefab;
    public List<Chunk> chunks = new List<Chunk>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeWorld();
    }
    
    public void InitializeWorld()
    {
        for (int x = -worldSizeRadius; x <= worldSizeRadius; x++)
        {
            for (int z = -worldSizeRadius; z <= worldSizeRadius; z++)
            {
                GameObject newChunk = Instantiate(chunkPrefab, transform);
                
                Chunk chunk = newChunk.GetComponent<Chunk>();
                chunk.chunkPosition = new Vector2Int(x,z);
                chunk.InitializeChunk();

                float height = Mathf.RoundToInt(MathUtils.Random01(x,z,seed)) * 2.5f;
                if (x == 0 && z == 0)
                    height = 0;
                
                Vector3 chunkWorldPosition = new Vector3(
                    chunk.chunkPosition.x * (int)Chunk.CHUNK_SIZE, 
                    height,
                    chunk.chunkPosition.y * (int)Chunk.CHUNK_SIZE
                    );
                
                newChunk.transform.position = chunkWorldPosition;

                newChunk.name = $"Chunk {chunk.chunkPosition}";
                chunks.Add(chunk);
            }
        }
    }

    public Chunk GetChunk(Vector3 worldPosition)
    {
        foreach (var chunk in chunks)
        {
            Vector3 chunkCenter = chunk.transform.position;
            float chunkSize = Chunk.CHUNK_SIZE;
            if (worldPosition.x < chunkCenter.x + chunkSize * 0.5f && worldPosition.z < chunkCenter.z + chunkSize * 0.5f)
            {
                return chunk;
            }
        }
        
        return null;
    }
}
