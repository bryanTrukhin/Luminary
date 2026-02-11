using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavNode
{
    public int clusterIndex;
    public Vector3 center;
    public Bounds bounds;

    //if it can move to another cluster
    [System.NonSerialized]
    public List<NavNode> neighbors = new List<NavNode>();


    //what a* needs
    [System.NonSerialized]
    public float gCost;
    [System.NonSerialized]
    public float hCost;
    public float fCost => gCost + hCost;

    [System.NonSerialized]
    public NavNode parent;

    public NavNode(int index, Vector3 center, Bounds bounds)
    {
        this.clusterIndex = index;
        this.center = center;
        this.bounds = bounds;
    }
}
