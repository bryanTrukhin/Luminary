using UnityEngine;
using System.Collections.Generic;
/*
Create Prefabs: Make Room_A, Room_B, Room_Start.

Add Markers: Open Room_A prefab. Add empty GameObjects. Attach SpawnMarker.

Set one to Type: Monster (Color Red).

Set one to Type: Loot (Color Green).

Setup Generator: Create an empty GameObject in your scene named LevelGenerator. Attach the script. Drag your room prefabs into the lists.
*/
public class LevelGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private int totalRooms = 5;
    [SerializeField] private float roomWidth = 20f; // Distance between room centers

    [Header("Room Blueprints")]
    [SerializeField] private GameObject startRoom;
    [SerializeField] private GameObject endRoom;
    [SerializeField] private List<GameObject> randomRooms;

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        Vector3 currentPos = transform.position;

        // 1. start Room
        SpawnRoom(startRoom, currentPos);
        currentPos.x += roomWidth;

        // 2. random middle room
        for (int i = 0; i < totalRooms; i++)
        {
            GameObject randomRoom = randomRooms[Random.Range(0, randomRooms.Count)];
            SpawnRoom(randomRoom, currentPos);
            currentPos.x += roomWidth;
        }

        // 3. end room
        SpawnRoom(endRoom, currentPos);
    }

    void SpawnRoom(GameObject roomPrefab, Vector3 position)
    {
        Instantiate(roomPrefab, position, Quaternion.identity, transform);
    }
}