using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }
    public int seed;
    public Action onTick;

    public GameObject islandPrefab;
    List<Island> islands = new List<Island>();
    
    int ticksPerSecond = 8;
    Coroutine tickCoroutine;

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
        if(seed == 0)
            seed = Random.Range(int.MinValue, int.MaxValue);
        StartTicks();
        CreateIsland(seed,Vector2Int.zero);
        CreateIsland(seed,Vector2Int.right);
    }

    public void CreateIsland(int seed, Vector2Int position)
    {
        // cada isla tiene un tamaño de 50x50
        GameObject go_island = Instantiate(islandPrefab, transform);
        
        Island island = go_island.GetComponent<Island>();
        island.InitializeIsland(position, seed);
        islands.Add(island);
    }
    
    public Chunk GetChunk(Vector3 worldPosition)
    {
        Island island = GetIsland(worldPosition);
        if (island != null)
        {
            foreach (var chunk in island.chunks)
            {
                if (chunk == null) continue; // chunk de agua
                Vector3 chunkCenter = chunk.transform.position;
                float chunkSize = Chunk.CHUNK_SIZE;
            
                // Verificamos si el worldPosition cae dentro de este chunk
                if (worldPosition.x >= chunkCenter.x - chunkSize / 2 &&
                    worldPosition.x < chunkCenter.x + chunkSize / 2 &&
                    worldPosition.z >= chunkCenter.z - chunkSize / 2 &&
                    worldPosition.z < chunkCenter.z + chunkSize / 2)
                {
                    return chunk;
                }
            }
        }

        // No hay chunk → es agua
        return null;
    }

    public Island GetIsland(Vector3 worldPosition)
    {
        float islandSize = Chunk.CHUNK_SIZE * (Island.ISLAND_RADIUS + 1) * 2;

        foreach (var island in islands)
        {
            Vector3 center = new Vector3(
                island.islandPosition.x * islandSize,
                0,
                island.islandPosition.y * islandSize
            );

            if (worldPosition.x >= center.x - islandSize / 2 && worldPosition.x < center.x + islandSize / 2 &&
                worldPosition.z >= center.z - islandSize / 2 && worldPosition.z < center.z + islandSize / 2)
            {
                return island;
            }
        }

        return null;
    }

    public void DestroyCellObject(Vector3 worldPosition)
    {
        Chunk chunk = GetChunk(worldPosition);
    
        if (chunk == null)
        {
            // chunk es agua, no hay objeto que destruir
            Debug.Log("Intentaste destruir un chunk de agua en " + worldPosition);
            return;
        }

        Vector2Int cellCoords = chunk.GetCellCoords(worldPosition);
        chunk.RemoveCellObject(cellCoords);
    }
    
    // ---------------- TICK SYSTEM ----------------

    public void StartTicks()
    {
        if (tickCoroutine == null)
            tickCoroutine = StartCoroutine(TickSystem());

        onTick += () =>
        {
            foreach (var island in islands)
            {
                foreach (var chunk in island.chunks)
                {
                    if (chunk == null) continue;

                    foreach (var building in chunk.chunkBuildings)
                    {
                        building.obj.GetComponent<IBuilding>().PlanTick();
                    }
                    
                    foreach (var building in chunk.chunkBuildings)
                    {
                        building.obj.GetComponent<IBuilding>().ActionTick();
                    }
                }
            }

        };
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
