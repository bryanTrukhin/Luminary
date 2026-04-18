using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityManager : MonoBehaviour
{
    public Transform player;
    public float activationRadius = 15f;
    public NavGraphBuilder navGraph;

    private List<GameObject> allAnts = new List<GameObject>();
    private GameObject currentlyActiveAnt;

    public void RegisterAnt(GameObject ant)
    {
        if (ant == null) return;
        allAnts.Add(ant);
        ant.SetActive(false);
    }

    void Update()
    {
        // 1. Clean up list (Robustness against destroyed ants)
        allAnts.RemoveAll(ant => ant == null);

        if (player == null) return;

        GameObject closestAnt = null;
        float minDistance = activationRadius;

        // 2. Find the single closest ant within radius
        foreach (GameObject ant in allAnts)
        {
            float dist = Vector3.Distance(player.position, ant.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestAnt = ant;
            }
        }

        // 3. Robust state switching
        if (closestAnt != currentlyActiveAnt)
        {
            // Disable previous
            if (currentlyActiveAnt != null)
                currentlyActiveAnt.SetActive(false);

            // Enable new winner
            currentlyActiveAnt = closestAnt;

            if (currentlyActiveAnt != null)
            {
                currentlyActiveAnt.SetActive(true);
                navGraph.enemy = currentlyActiveAnt.transform;
            }
            else
            {
                navGraph.enemy = null; // Clear graph if none in range
            }
        }
    }
}