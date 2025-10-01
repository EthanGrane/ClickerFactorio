using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Island : MonoBehaviour
{
    public Vector2Int islandPosition;

    [Header("Biome Generation")]
    public BiomeRule[] biomes;             // Reglas configurables en inspector
    Dictionary<string, int> biomeCounters = new Dictionary<string, int>();

    [Range(0f,1f)] public float waterPercentage = 0.3f;
    
    public List<Chunk> chunks = new List<Chunk>();
    public const int ISLAND_RADIUS = 2;
    int seed;

    public void InitializeIsland(Vector2Int islandPosition, int seed)
    {
        this.islandPosition = islandPosition;
        this.seed = seed;
        
        InitializeChunks();
    }
    void InitializeChunks()
{
    int totalChunks = (ISLAND_RADIUS * 2 + 1) * (ISLAND_RADIUS * 2 + 1);

    // Inicializar contadores
    biomeCounters.Clear();
    foreach (var b in biomes)
        biomeCounters[b.name] = 0;

    // Inicializar lista de chunks con null (vacíos)
    chunks = new List<Chunk>(new Chunk[totalChunks]);

    // Generar posiciones de la cuadrícula
    List<Vector2Int> positions = new List<Vector2Int>();
    for (int x = -ISLAND_RADIUS; x <= ISLAND_RADIUS; x++)
        for (int z = -ISLAND_RADIUS; z <= ISLAND_RADIUS; z++)
            positions.Add(new Vector2Int(x, z));

    float emptyChance = waterPercentage;
    int index = 0;

    foreach (var pos in positions)
    {
        // Decidir si este chunk será vacío
        if (MathUtils.Random01(pos.x, pos.y, seed) < emptyChance)
        {
            chunks[index] = null; // chunk vacío
            index++;
            continue;
        }

        float dist = pos.magnitude;
        float distNormalized = Mathf.Clamp01(dist / ISLAND_RADIUS);
        float falloff = 1f - Mathf.Pow(distNormalized, 2);

        // Calcular pesos solo para biomas que no excedan maxChunks
        float totalWeight = 0f;
        foreach (var b in biomes)
        {
            if (biomeCounters[b.name] >= b.maxChunks) continue;
            float weight = Mathf.Clamp01(1f - Mathf.Abs(falloff - b.spawnBias));
            totalWeight += weight;
        }

        BiomeRule? chosenBiome = null;

        if (totalWeight > 0f)
        {
            float r = MathUtils.Random01(pos.x, pos.y, seed) * totalWeight;
            foreach (var b in biomes)
            {
                if (biomeCounters[b.name] >= b.maxChunks) continue;
                float weight = Mathf.Clamp01(1f - Mathf.Abs(falloff - b.spawnBias));
                if (r < weight)
                {
                    chosenBiome = b;
                    break;
                }
                r -= weight;
            }
        }

        GameObject prefabToUse = chosenBiome?.prefab;

        if (prefabToUse != null)
        {
            GameObject newChunkGO = Instantiate(prefabToUse, transform);
            Chunk chunk = newChunkGO.GetComponent<Chunk>();
            chunk.chunkPosition = pos;
            chunk.InitializeChunk(0, chunk.height);

            // Posición en mundo
            Vector3 chunkWorldPosition = new Vector3(
                chunk.chunkPosition.x * Chunk.CHUNK_SIZE,
                chunk.height,
                chunk.chunkPosition.y * Chunk.CHUNK_SIZE
            );
            Vector3 center = new Vector3(
                islandPosition.x * (Chunk.CHUNK_SIZE * (ISLAND_RADIUS + 1) * 2),
                0,
                islandPosition.y * (Chunk.CHUNK_SIZE * (ISLAND_RADIUS + 1) * 2)
            );
            chunkWorldPosition += center;
            newChunkGO.transform.position = chunkWorldPosition - Vector3.up * 20f;
            newChunkGO.transform.DOMove(chunkWorldPosition, 1f);
            newChunkGO.name = $"Chunk {chunk.chunkPosition}";

            chunks[index] = chunk;

            if (chosenBiome.HasValue)
                biomeCounters[chosenBiome.Value.name]++;
        }
        else
        {
            // Agua o vacío
            chunks[index] = null;
        }

        index++;
    }

    // Garantizar minChunks
    foreach (var b in biomes)
    {
        while (biomeCounters[b.name] < b.minChunks)
        {
            int randIndex = Random.Range(0, chunks.Count);
            Chunk replaceChunk = chunks[randIndex];
            if (replaceChunk == null) continue; // nunca reemplazar vacío

            // Reducir contador del bioma reemplazado
            foreach (var bb in biomes)
            {
                if (replaceChunk.gameObject.name.Contains(bb.name))
                {
                    biomeCounters[bb.name]--;
                    break;
                }
            }

            // Reemplazar con el bioma que falta
            GameObject newChunkGO = Instantiate(b.prefab, transform);
            Chunk newChunk = newChunkGO.GetComponent<Chunk>();
            newChunk.chunkPosition = replaceChunk.chunkPosition;
            newChunk.InitializeChunk(0, newChunk.height);
            newChunkGO.transform.position = replaceChunk.transform.position;

            Destroy(replaceChunk.gameObject);
            chunks[randIndex] = newChunk;
            biomeCounters[b.name]++;
        }
    }
}

    

    void OnDrawGizmos()
    {
        Vector3 center = new Vector3(
            islandPosition.x * (Chunk.CHUNK_SIZE * (ISLAND_RADIUS + 1) * 2),
            0,
            islandPosition.y * (Chunk.CHUNK_SIZE * (ISLAND_RADIUS + 1) * 2)
        );

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, Vector3.one * Chunk.CHUNK_SIZE * (ISLAND_RADIUS + 1) * 2);
    }
}

[System.Serializable]
public struct BiomeRule
{
    public string name;           // Solo informativo
    public GameObject prefab;     // Prefab del chunk a instanciar

    [Range(0f, 1f)] public float spawnBias;       // 1 = centro, 0 = borde
    
    public int minChunks;         // mínimo de chunks en todo el mundo
    public int maxChunks;         // máximo de chunks en todo el mundo
}
