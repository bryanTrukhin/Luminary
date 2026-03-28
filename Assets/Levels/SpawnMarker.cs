using UnityEngine;
using System.Collections.Generic;

public enum SpawnType { Monster, SafeZone, Loot, Trap }

public class SpawnMarker : MonoBehaviour
{
    [Header("Settings")]
    public SpawnType type;
    public Color gizmoColor = Color.red;
    
    [Header("RNG Rules")]
    [Tooltip("If true, ignores chance and always spawns.")]
    public bool isGuaranteed = false;

    [Range(0f, 100f)]
    [Tooltip("Chance to spawn (0 to 100).")]
    public float spawnChance = 50f;

    [Header("What to Spawn")]
    [Tooltip("The script will pick ONE random prefab from this list.")]
    public List<GameObject> possiblePrefabs = new List<GameObject>();

    // Visual helper for the Editor
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.4f);
        // Draw facing direction
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, transform.right * 0.5f);
    }
}