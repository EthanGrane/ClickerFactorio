using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    public int worldSizeRadius = 16;
    public int seed;

    public int ticksPerSecond = 4;
    public Action onTick;

    public GameObject chunkPrefab;
    public List<Chunk> chunks = new List<Chunk>();

    private Coroutine tickCoroutine;

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
        StartTicks();
    }

    public void InitializeWorld()
    {
        for (int x = -worldSizeRadius; x <= worldSizeRadius; x++)
        {
            for (int z = -worldSizeRadius; z <= worldSizeRadius; z++)
            {
                float height = Mathf.RoundToInt(MathUtils.Random01(x, z, seed)) * 2.5f;
                if (x == 0 && z == 0)
                    height = 0;
                
                GameObject newChunk = Instantiate(chunkPrefab, transform);

                Chunk chunk = newChunk.GetComponent<Chunk>();
                chunk.chunkPosition = new Vector2Int(x, z);
                chunk.InitializeChunk(0,height);

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

    // ---------------- TICK SYSTEM ----------------

    public void StartTicks()
    {
        if (tickCoroutine == null)
            tickCoroutine = StartCoroutine(TickSystem());
    }

    public void StopTicks()
    {
        if (tickCoroutine != null)
        {
            StopCoroutine(tickCoroutine);
            tickCoroutine = null;
        }
    }

    private IEnumerator TickSystem()
    {
        if (ticksPerSecond <= 0)
        {
            Debug.LogWarning("Ticks per second must be greater than 0.");
            yield break;
        }

        float interval = 1f / ticksPerSecond;
        float nextTickTime = Time.realtimeSinceStartup + interval;

        while (true)
        {
            float waitTime = nextTickTime - Time.realtimeSinceStartup;
            if (waitTime > 0f)
                yield return new WaitForSecondsRealtime(waitTime);

            onTick?.Invoke();
            nextTickTime += interval;
        }
    }
}
