using UnityEngine;
using System.Collections.Generic;

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
        public int repetitions = 1; // How many times to spawn this?

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

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        // Setup initial spawn point
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
                    // Find the matching pool
                    roomToSpawn = GetRandomRoomFromPool(step.poolIDToUse);
                }

                if (roomToSpawn != null)
                {
                    SpawnAndConnect(roomToSpawn);
                }
            }
        }
        
        Destroy(startPointObj);
    }

    // Helper to find the right list and pick a random room
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
        // Instantiate
        GameObject newRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, transform);

        // Find markers
        Transform markers = newRoom.transform.Find("Markers");
        if(markers == null) { Debug.LogError("Room missing Markers!"); return; }

        Transform entrance = markers.Find("Entrance");
        Transform exit = markers.Find("Exit");

        // Align
        Vector3 displacement = currentExitPoint.position - entrance.position;
        newRoom.transform.position += displacement;

        // Advance
        currentExitPoint = exit;
    }
}