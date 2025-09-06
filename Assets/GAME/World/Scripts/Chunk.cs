using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Vector2Int chunkPosition = Vector2Int.zero;
    private CellObject[,] cellData = new CellObject[CHUNK_CELL_SIZE, CHUNK_CELL_SIZE];
    [Space]
    public CellObject[] ChunkObjectsPrefabs;

    public static float CHUNK_SIZE = 10f;
    public static int CHUNK_CELL_SIZE = 8;

    public void InitializeChunk(int seed = 0)
    {
        // Inicializamos la grid vacía
        for (int x = 0; x < CHUNK_CELL_SIZE; x++)
            for (int z = 0; z < CHUNK_CELL_SIZE; z++)
                cellData[x, z] = null;

        // Generamos algunos objetos en el centro del chunk
        if (chunkPosition == Vector2Int.zero)
        {
            int start = (CHUNK_CELL_SIZE / 2) - 2;
            int end = (CHUNK_CELL_SIZE / 2) + 2;
            for (int x = start; x < end; x++)
            for (int z = start; z < end; z++)
            {
                CellObject objData = new CellObject(ChunkObjectsPrefabs[0]);

                cellData[x, z] = objData;
            }
        }


        // Spawn aleatorio de objetos
        for (int i = 0; i < 5; i++)
        {
            float random = MathUtils.Random01(chunkPosition.x * 1000, chunkPosition.y * 1000, seed);
            CellObject objData = new CellObject(ChunkObjectsPrefabs[Mathf.FloorToInt(random * ChunkObjectsPrefabs.Length)]);
            objData.rotation = UnityEngine.Random.Range(0, 3);
            Vector2Int? cellPos = GetRandomEmptyCellForSize(objData.size, seed);
            if (!cellPos.HasValue) continue;

            PlaceCellObject(cellPos.Value, objData);
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
    public CellObject PlaceCellObject(Vector2Int pivot, CellObject template)
    {
        CellObject objData = new CellObject(template);
        int size = objData.size; // Asumimos que siempre es cuadrado

        bool canPlace = CanPlaceCellObject(pivot, size);
        if (!canPlace) return null;
        
        // Instancia el objeto en el mundo
        GameObject go = Instantiate(objData.prefab);

        // Calcula posición centrada sobre el bloque de celdas
        float cellSpacing = CHUNK_SIZE / CHUNK_CELL_SIZE;
        Vector3 centerOffset = new Vector3(size * 0.5f * cellSpacing, 0, size * 0.5f * cellSpacing);
        Vector3 basePos = GetCellWorldPosition(pivot);
        go.transform.position = basePos + (centerOffset - new Vector3(cellSpacing * 0.5f, 0, cellSpacing * 0.5f));

        go.transform.rotation = Quaternion.identity;
        go.transform.SetParent(transform);

        // Inicializa la data
        objData.Initialize(chunk: this, obj: go, position: pivot, rotation: 0);

        // Marca todas las celdas que ocupa como ocupadas
        for (int x = 0; x < size; x++)
        for (int z = 0; z < size; z++)
        {
            int cx = pivot.x + x;
            int cz = pivot.y + z;
            cellData[cx, cz] = objData;
        }

        return objData;
    }

    public bool CanPlaceCellObject(Vector2Int pivot, int size)
    {
        // Verifica que todas las celdas necesarias estén dentro del chunk y libres
        for (int x = 0; x < size; x++)
        for (int z = 0; z < size; z++)
        {
            int cx = pivot.x + x;
            int cz = pivot.y + z;

            if (cx < 0 || cx >= CHUNK_CELL_SIZE || cz < 0 || cz >= CHUNK_CELL_SIZE)
                return false; // fuera del chunk
            if (cellData[cx, cz] != null)
                return false; // ya ocupada
        }

        return true; // todas las celdas están libres
    }

    public static int GetRotatedSize(int size, int rotation)
    {
        rotation = rotation % 4;
        if (rotation == 1 || rotation == 3) // 90° o 270°
            return size;
        
        return size; // 0° o 180°
    }

    public void RemoveCellBuilding(Vector2Int cellPos)
    {
        CellObject removeObject = cellData[cellPos.x, cellPos.y];
        if(removeObject == null) return;
        
        if (removeObject.type == CellType.Building)
        {
            Destroy(removeObject.obj);
            removeObject = null;
            cellData[cellPos.x, cellPos.y] = null;
        }
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
        int rotatedSize = GetRotatedSize(size, rotation);

        // Calculamos offset respecto al pivot
        int offsetX = 0;
        int offsetZ = 0;

        switch(rotation % 4)
        {
            case 0: // 0° 
                offsetX = 0;
                offsetZ = 0;
                break;
            case 1: // 90° 
                offsetX = 0;
                offsetZ = -(rotatedSize - 1);
                break;
            case 2: // 180°
                offsetX = -(rotatedSize - 1);
                offsetZ = -(rotatedSize - 1);
                break;
            case 3: // 270°
                offsetX = -(rotatedSize - 1);
                offsetZ = 0;
                break;
        }

        float worldX = (pivotCell.x + offsetX + rotatedSize * 0.5f) * cellSpacing + chunkPosition.x - Chunk.CHUNK_SIZE / 2f;
        float worldZ = (pivotCell.y + offsetZ + rotatedSize * 0.5f) * cellSpacing + chunkPosition.z - Chunk.CHUNK_SIZE / 2f;

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
