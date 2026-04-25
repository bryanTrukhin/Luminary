using System.Collections.Generic;
using UnityEngine;

public class ProximityManager : MonoBehaviour
{
    public Transform player;
    public float activationRadius = 15f;
    public NavGraphBuilder navGraph;

    private List<GameObject> allEnemies = new List<GameObject>();
    private GameObject currentlyActiveEnemy;

    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        allEnemies.Add(enemy);
        enemy.SetActive(false);
    }

    void Update()
    {
        // 1. Clean up list
        allEnemies.RemoveAll(e => e == null);

        if (player == null) return;

        GameObject closestEnemy = null;
        float minDistance = activationRadius;

        // 2. Find closest enemy within radius
        foreach (GameObject enemy in allEnemies)
        {
            float dist = Vector3.Distance(player.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestEnemy = enemy;
            }
        }

        // 3. State switching
        if (closestEnemy != currentlyActiveEnemy)
        {
            if (currentlyActiveEnemy != null)
                currentlyActiveEnemy.SetActive(false);

            currentlyActiveEnemy = closestEnemy;

            if (currentlyActiveEnemy != null)
            {
                currentlyActiveEnemy.SetActive(true);
                if (navGraph != null) navGraph.enemy = currentlyActiveEnemy.transform;
            }
            else
            {
                if (navGraph != null) navGraph.enemy = null;
            }
        }
    }
}