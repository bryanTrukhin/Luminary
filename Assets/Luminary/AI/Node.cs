using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector2 worldPos;
    public int gridX, gridY;

    public int gCost;
    public int hCost;
    public Node parent;

    public int fCost => gCost + hCost;

    public Node(bool walkable, Vector2 worldPos, int x, int y)
    {
        this.walkable = walkable;
        this.worldPos = worldPos;
        gridX = x;
        gridY = y;
    }
}
