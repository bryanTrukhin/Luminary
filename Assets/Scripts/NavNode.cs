using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavNode
{
    public int clusterIndex;
    public Vector3 center;
    public Bounds bounds;

    public float gCost;
    public float hCost;
    public float fCost => gCost + hCost;
    public NavNode parent;

    public NavNode(int index, Vector3 center, Bounds bounds)
    {
        this.clusterIndex = index;
        this.center = center;
        this.bounds = bounds;
    }
}