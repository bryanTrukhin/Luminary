using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlatformCluster
{
    public int id;
    public Vector3 center;

    // tiles that belong to this platform
    public HashSet<Vector3Int> tiles = new HashSet<Vector3Int>();

    // walkable surface nodes
    public List<TileNode> tileNodes = new List<TileNode>();

    // jump exits (later)
    public List<TileNode> exits = new List<TileNode>();
}
