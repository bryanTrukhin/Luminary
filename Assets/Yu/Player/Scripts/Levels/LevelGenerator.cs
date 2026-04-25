using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [System.Serializable]
    public class RoomPool
    {
        public string poolID;
        public List<GameObject> rooms;
    }

    [System.Serializable]
    public class LevelStep
    {
        public string stepName = "Step";
        public enum StepType { FixedRoom, FromPool }
        public StepType type;

        [Range(1, 20)]
        public int repetitions = 1;

        [Header("If Fixed")]
        public GameObject specificRoom;

        [Header("If From Pool")]
        public string poolIDToUse;
    }

    [Header("Level Collections")]
    public List<RoomPool> roomPalettes;

    [Header("Sequence")]
    public List<LevelStep> generationSequence;

    private Transform currentExitPoint;
    public ProximityManager proxManager;


    void Awake()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        GameObject startPointObj = new GameObject("GenerationStartPoint");
        startPointObj.transform.position = transform.position;
        currentExitPoint = startPointObj.transform;

        foreach (LevelStep step in generationSequence)
        {
            for (int i = 0; i < step.repetitions; i++)
            {
                GameObject roomToSpawn = null;

                if (step.type == LevelStep.StepType.FixedRoom)
                {
                    roomToSpawn = step.specificRoom;
                }
                else if (step.type == LevelStep.StepType.FromPool)
                {
                    roomToSpawn = GetRandomRoomFromPool(step.poolIDToUse);
                }

                if (roomToSpawn != null)
                {
                    SpawnAndConnect(roomToSpawn);
                }
            }
        }

        Destroy(startPointObj);
        MergeGroundTilemaps();
    }

    GameObject GetRandomRoomFromPool(string id)
    {
        foreach (RoomPool pool in roomPalettes)
        {
            if (pool.poolID == id)
            {
                if (pool.rooms.Count > 0)
                    return pool.rooms[Random.Range(0, pool.rooms.Count)];
                else
                    Debug.LogWarning("Pool '" + id + "' is empty!");
            }
        }
        Debug.LogError("Could not find a Room Pool with ID: " + id);
        return null;
    }

    void SpawnAndConnect(GameObject roomPrefab)
    {
        GameObject newRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, transform);

        Transform markers = newRoom.transform.Find("Markers");
        if (markers == null) { Debug.LogError("Room missing Markers!"); return; }

        Transform entrance = markers.Find("Entrance");
        Transform exit = markers.Find("Exit");
        Vector3 displacement = currentExitPoint.position - entrance.position;
        newRoom.transform.position += displacement;

        currentExitPoint = exit;

        // Register Ants
        AntMoving[] antsInRoom = newRoom.GetComponentsInChildren<AntMoving>(true);
        foreach (AntMoving ant in antsInRoom)
        {
            proxManager.RegisterEnemy(ant.gameObject);
        }

    }

    void MergeGroundTilemaps()
    {
        GameObject gridObj = new GameObject("MergedGrid");
        gridObj.transform.parent = transform;
        gridObj.AddComponent<Grid>();

        // 1. Merge Platform / Ground
        GameObject[] groundObjects = GameObject.FindGameObjectsWithTag("Platform");
        Tilemap groundDest = CreateMergedTilemap(gridObj.transform, "MergedGroundTilemap", "Platform", "Ground", groundObjects);

        // 2. Merge Boundary / Water
        List<GameObject> boundaryObjects = new List<GameObject>();
        int waterLayer = LayerMask.NameToLayer("Water");

        Tilemap[] allMaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        foreach (Tilemap tm in allMaps)
        {
            if (tm.name.ToLower().Contains("boundary") && tm.gameObject.layer == waterLayer)
            {
                boundaryObjects.Add(tm.gameObject);
            }
        }

        CreateMergedTilemap(gridObj.transform, "MergedBoundaryTilemap", "Untagged", "Water", boundaryObjects.ToArray());

        // 3. Initialize pathfinding strictly on the Ground tilemap
        PlatformClustering clustering = GetComponent<PlatformClustering>();
        if (clustering != null && groundDest != null)
        {
            clustering.Initialize(groundDest);
            NavGraphBuilder navGraph = GetComponent<NavGraphBuilder>();
            if (navGraph != null) navGraph.Initialize(clustering);
        }
    }

    // Extracted helper function for batching multiple types of tilemaps
    Tilemap CreateMergedTilemap(Transform parentGrid, string objName, string tag, string layerName, GameObject[] sources)
    {
        GameObject mergedObj = new GameObject(objName);
        mergedObj.transform.parent = parentGrid;
        mergedObj.tag = tag;

        int layer = LayerMask.NameToLayer(layerName);
        if (layer != -1) mergedObj.layer = layer;

        Tilemap destination = mergedObj.AddComponent<Tilemap>();
        TilemapRenderer renderer = mergedObj.AddComponent<TilemapRenderer>();

        if (sources.Length > 0)
        {
            TilemapRenderer sourceRenderer = sources[0].GetComponent<TilemapRenderer>();
            if (sourceRenderer != null)
            {
                renderer.sortingLayerID = sourceRenderer.sortingLayerID;
                renderer.sortingOrder = sourceRenderer.sortingOrder;
            }
        }

        TilemapCollider2D tileCollider = mergedObj.AddComponent<TilemapCollider2D>();
        CompositeCollider2D compCollider = mergedObj.AddComponent<CompositeCollider2D>();
        tileCollider.usedByComposite = true;
        mergedObj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        List<Vector3Int> newPositions = new List<Vector3Int>();
        List<TileBase> newTiles = new List<TileBase>();

        foreach (GameObject go in sources)
        {
            Tilemap source = go.GetComponent<Tilemap>();
            if (source == null) continue;

            BoundsInt bounds = source.cellBounds;
            TileBase[] block = source.GetTilesBlock(bounds);
            int index = 0;

            foreach (Vector3Int localPos in bounds.allPositionsWithin)
            {
                TileBase tile = block[index];
                if (tile != null)
                {
                    Vector3 worldPos = source.CellToWorld(localPos);
                    Vector3Int destPos = destination.WorldToCell(worldPos);
                    newPositions.Add(destPos);
                    newTiles.Add(tile);
                }
                index++;
            }

            go.SetActive(false);
        }

        destination.SetTiles(newPositions.ToArray(), newTiles.ToArray());
        return destination;
    }
}