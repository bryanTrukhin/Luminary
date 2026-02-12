using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileNode
{
    public Vector3Int cellPos;
    public Vector3 worldPos;

    public float gCost;
    public float hCost;
    public float fCost => gCost + hCost;
    public TileNode parent;

    public List<TileNode> neighbors = new List<TileNode>();
}
