using UnityEngine;

public class RoomController : MonoBehaviour
{
    // If true, the room decorates itself immediately when spawned.
    // If false, call InitializeRoom() manually from the LevelGenerator.
    [SerializeField] private bool autoInitialize = true;

    void Start()
    {
        if (autoInitialize) InitializeRoom();
    }

    public void InitializeRoom()
    {
        // Find all markers strictly inside this room
        SpawnMarker[] markers = GetComponentsInChildren<SpawnMarker>();

        foreach (var marker in markers)
        {
            if (!marker.isGuaranteed && Random.Range(0f, 100f) > marker.spawnChance)
            {
                continue;
            }
            if (marker.possiblePrefabs.Count == 0) continue;
            GameObject prefabToSpawn = marker.possiblePrefabs[Random.Range(0, marker.possiblePrefabs.Count)];
        
            // Parent it to this Room so everything stays organized in hierarchy
            Collider2D hit = Physics2D.OverlapCircle(marker.transform.position, 1.0f);
            if (hit != null && hit.CompareTag("SafeZone")) 
            {
                // Don't spawn here!
                continue;
            }
            
            Instantiate(prefabToSpawn, marker.transform.position, marker.transform.rotation, this.transform);
        }
    }
}