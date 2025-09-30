using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Chunk : MonoBehaviour
{
    public Vector2Int chunkPosition = Vector2Int.zero;
    public CellObject[,] cellData = new CellObject[CHUNK_CELL_SIZE, CHUNK_CELL_SIZE];
    [Space]
    public CellObject[] ChunkObjectsPrefabs;
    public List<CellObject> chunkBuildings = new List<CellObject>();
    public List<CellObject> chunkResources = new List<CellObject>();
    public float height = 0;

    private int seed;
    public static int MAX_RESOURCES_ON_CHUNK = 8;
    public static float CHUNK_SIZE = 10f;
    public static int CHUNK_CELL_SIZE = 8;
    public static float CHUNK_SPACING(){return CHUNK_SIZE/CHUNK_CELL_SIZE;}

    public void InitializeChunk(int seed = 0, float height = 0)
    {
        this.seed = seed;
        this.height = height;
        // Inicializamos la grid vacía
        for (int x = 0; x < CHUNK_CELL_SIZE; x++)
            for (int z = 0; z < CHUNK_CELL_SIZE; z++)
                cellData[x, z] = null;
        
        for (int x = 0; x < 3; x++)
            SpawnRandomResource();
        
        StartCoroutine(SpawnRandomResourceCoroutine());
    }

    public CellObject GetCellObject(Vector2Int cellPos)
    {
        if(cellPos.x < 0 || cellPos.x >= CHUNK_CELL_SIZE || cellPos.y < 0 || cellPos.y >= CHUNK_CELL_SIZE)
            return null;

        return cellData[cellPos.x, cellPos.y];
    }

    void SpawnRandomResource()
    {
        // Spawn aleatorio de objetos
        for (int i = 0; i < 5; i++)
        {
            float random = Random.Range(0f, 1f);
            CellObject objData = new CellObject(ChunkObjectsPrefabs[Mathf.FloorToInt(random * ChunkObjectsPrefabs.Length)]);
            objData.rotation = Random.Range(0, 3);
            Vector2Int? cellPos = GetRandomEmptyCellForSize(objData.size, seed);
            if (!cellPos.HasValue) continue;

            CellObject cellObject = PlaceCellObject(cellPos.Value, objData);
            chunkResources.Add(cellObject);
            break;
        }
    }

    IEnumerator SpawnRandomResourceCoroutine()
    {
        while (true)
        {
            float random = UnityEngine.Random.Range(3, 5);
            yield return new WaitForSeconds(random);
            
            if(chunkResources.Count < MAX_RESOURCES_ON_CHUNK)
                SpawnRandomResource();
        }
    }

    // Función que devuelve una celda aleatoria vacía para un tamaño
    public Vector2Int? GetRandomEmptyCellForSize(int size, int seed = 0)
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int x = 0; x <= CHUNK_CELL_SIZE - size; x++)
            for (int z = 0; z <= CHUNK_CELL_SIZE - size; z++)
            {
                bool canPlace = true;
                for (int ix = 0; ix < size && canPlace; ix++)
                    for (int iz = 0; iz < size && canPlace; iz++)
                        if (cellData[x + ix, z + iz] != null)
                            canPlace = false;

                if (canPlace)
                {
                    float random = MathUtils.Random01(x, z, seed);
                    if (random > 0.5f) emptyCells.Add(new Vector2Int(x, z));
                }
            }

        if (emptyCells.Count == 0) return null;

        float r = MathUtils.Random01(chunkPosition.x, chunkPosition.y, seed);
        int index = Mathf.FloorToInt(r * emptyCells.Count);
        return emptyCells[index];
    }

    // Verifica si se puede colocar un objeto de tamaño `size` en `cellPos`
    // Coloca un objeto en la grid y lo instancia en el mundo (versión simple)
    public CellObject PlaceCellObject(Vector2Int pivot, CellObject building)
    {
        CellObject objData = new CellObject(building);
        int size = objData.size; // Asumimos que siempre es cuadrado

        bool canPlace = CanPlaceCellObject(pivot, size, building.rotation);
        if (!canPlace) return null;
        
        // Instancia el objeto en el mundo
        GameObject go = Instantiate(objData.prefab);

        // Calcula posición centrada sobre el bloque de celdas
        Vector3 placementPosition = GetPlacementWorldPosition(pivot, building.size, transform.position, building.rotation);
        go.transform.position = placementPosition;

        go.transform.rotation = Quaternion.Euler(Vector3.up * (90f * building.rotation));
        go.transform.SetParent(transform);

        Vector3 objectScale = go.transform.localScale;
        go.transform.localScale = Vector3.zero;
        go.transform.DOScale(objectScale, 0.25f).SetEase(Ease.OutExpo);
        
        // Inicializa la data
        objData.Initialize(chunk: this, obj: go, position: pivot, rotation: building.rotation);
        go.name = $"{objData.prefab.name} ({pivot.x}, {pivot.y})";
        
        // Marca todas las celdas que ocupa como ocupadas
        for (int x = 0; x < size; x++)
        for (int z = 0; z < size; z++)
        {
            int cx = pivot.x;
            int cz = pivot.y;

            switch(building.rotation % 4)
            {
                case 0: cx += x; cz += z; break;                    // 0°
                case 1: cx += z; cz += (size - 1 - x); break;      // 90°
                case 2: cx += (size - 1 - x); cz += (size - 1 - z); break; // 180°
                case 3: cx += (size - 1 - z); cz += x; break;      // 270°
            }

            // Fuera del rango
            if (cx < 0 || cx >= CHUNK_CELL_SIZE || cz < 0 || cz >= CHUNK_CELL_SIZE)
            {
                Vector3 worldPos = GetCellWorldPosition(new Vector2Int(cx, cz));
                Chunk chunk = WorldManager.Instance.GetChunk(worldPos);
                
                Vector2Int cellPos = chunk.GetCellCoords(worldPos);
                chunk.cellData[cellPos.x, cellPos.y] = objData;
                
            }
            else
                cellData[cx, cz] = objData;
        }
        
        if(objData.type == CellType.Building)
        {
            chunkBuildings.Add(objData);
            objData.obj.GetComponent<IBuilding>().Initialize(objData);
        }

        return objData;
    }

    public bool CanPlaceCellObject(Vector2Int pivot, int size, int rotation)
    {

        for (int x = 0; x < size; x++)
        for (int z = 0; z < size; z++)
        {
            int cx = pivot.x;
            int cz = pivot.y;

            switch(rotation % 4)
            {
                case 0: cx += x; cz += z; break;
                case 1: cx += z; cz += (size - 1 - x); break;
                case 2: cx += (size - 1 - x); cz += (size - 1 - z); break;
                case 3: cx += (size - 1 - z); cz += x; break;
            }


            // Fuera del rango
            if (cx < 0 || cx >= CHUNK_CELL_SIZE || cz < 0 || cz >= CHUNK_CELL_SIZE)
            {
                Vector3 worldPos = GetCellWorldPosition(new Vector2Int(cx, cz));
                Chunk chunk = WorldManager.Instance.GetChunk(worldPos);
                
                if (chunk == null) return false;

                if (chunk.height != height)
                    return false;
                
                Vector2Int cellPos = chunk.GetCellCoords(worldPos);
                if(chunk.cellData[cellPos.x, cellPos.y] == null) return true;
                
                return false;
            }

            // Celda ya ocupada
            if (cellData[cx, cz] != null)
            {
                return false;
            }
        }

        return true;
    }
    
    public CellObject RemoveCellObject(Vector2Int cellPos)
    {
        CellObject removeObject = cellData[cellPos.x, cellPos.y];
        if(removeObject == null) return null;

        removeObject.Destroy();
        
        switch (removeObject.type)
        {
            case CellType.Empty:
                break;
            case CellType.Building:
                chunkBuildings.Remove(removeObject);
                Destroy(removeObject.obj);
            
                // Marca todas las celdas que ocupa como ocupadas
                Debug.Log($"Removing {removeObject.type} at {removeObject.position} size={removeObject.size} rot={removeObject.rotation}");
                for (int x = 0; x < removeObject.size; x++)
                for (int z = 0; z < removeObject.size; z++)
                {
                    int cx = removeObject.position.x;
                    int cz = removeObject.position.y;
                    int size = removeObject.size;

                    switch(removeObject.rotation % 4)
                    {
                        case 0: cx += x; cz += z; break;
                        case 1: cx += z; cz += (size - 1 - x); break;
                        case 2: cx += (size - 1 - x); cz += (size - 1 - z); break;
                        case 3: cx += (size - 1 - z); cz += x; break;
                    }

                    Debug.Log($"Clearing cellData[{cx},{cz}]");
                    cellData[cx, cz] = null;
                }
                break;
            case CellType.Resource:
                
                chunkResources.Remove(removeObject);
                Destroy(removeObject.obj);
            
                // Marca todas las celdas que ocupa como ocupadas
                for (int x = 0; x < removeObject.size; x++)
                for (int z = 0; z < removeObject.size; z++)
                {
                    int cx = removeObject.position.x;
                    int cz = removeObject.position.y;
                    int size = removeObject.size;

                    switch(removeObject.rotation % 4)
                    {
                        case 0: cx += x; cz += z; break;
                        case 1: cx += z; cz += (size - 1 - x); break;
                        case 2: cx += (size - 1 - x); cz += (size - 1 - z); break;
                        case 3: cx += (size - 1 - z); cz += x; break;
                    }

                    cellData[cx, cz] = null;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return removeObject;
    }

    // Convierte posición del mundo a coordenadas de celda
    public Vector2Int GetCellCoords(Vector3 worldPos)
    {
        float cellSpacing = CHUNK_SIZE / CHUNK_CELL_SIZE;
        Vector3 localPos = worldPos - transform.position;
        localPos.x += CHUNK_SIZE * 0.5f;
        localPos.z += CHUNK_SIZE * 0.5f;

        int cellX = Mathf.FloorToInt(localPos.x / cellSpacing);
        int cellZ = Mathf.FloorToInt(localPos.z / cellSpacing);
        return new Vector2Int(cellX, cellZ);
    }

    // Convierte coordenadas de celda a posición en el mundo
    public Vector3 GetCellWorldPosition(Vector2Int cellPos)
    {
        float cellSpacing = CHUNK_SIZE / CHUNK_CELL_SIZE;
        Vector3 center = transform.position;
        float worldX = (cellPos.x * cellSpacing + center.x) - (CHUNK_SIZE * 0.5f) + cellSpacing * 0.5f;
        float worldZ = (cellPos.y * cellSpacing + center.z) - (CHUNK_SIZE * 0.5f) + cellSpacing * 0.5f;
        return new Vector3(worldX, center.y, worldZ);
    }
    
    public static Vector3 GetPlacementWorldPosition(Vector2Int pivotCell, int size, Vector3 chunkPosition, int rotation = 0)
    {
        float cellSpacing = Chunk.CHUNK_SIZE / Chunk.CHUNK_CELL_SIZE;
        Vector3 center = chunkPosition;

        Vector3 offset = new Vector3(size * 0.5f * cellSpacing, 0, size * 0.5f * cellSpacing);

        float worldX = pivotCell.x * cellSpacing + center.x - Chunk.CHUNK_SIZE/2 + offset.x;
        float worldZ = pivotCell.y * cellSpacing + center.z - Chunk.CHUNK_SIZE/2 + offset.z;

        return new Vector3(worldX, chunkPosition.y, worldZ);
    }
    
    private void OnDrawGizmos()
    {
        float cellSpacing = CHUNK_SIZE / CHUNK_CELL_SIZE;
        Vector3 center = transform.position;

        for (int x = 0; x < CHUNK_CELL_SIZE; x++)
            for (int z = 0; z < CHUNK_CELL_SIZE; z++)
            {
                Gizmos.color = cellData[x, z] == null ? Color.green : Color.red;
                Gizmos.DrawCube(
                    new Vector3(
                        (x * cellSpacing + center.x) - (CHUNK_SIZE * 0.5f) + cellSpacing * 0.5f,
                        transform.position.y,
                        (z * cellSpacing + center.z) - (CHUNK_SIZE * 0.5f) + cellSpacing * 0.5f
                    ),
                    Vector3.one * 0.25f
                );
            }
    }
}
