using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NavGraphBuilder : MonoBehaviour
{
    public PlatformClustering clustering;
    public Tilemap tilemap;

    public List<NavNode> nodes = new List<NavNode>();

    void Start()
    {
        Debug.Log("NavGB Start");

        Debug.Log("clustering is " + (clustering == null ? "NULL" : "OK"));
        Debug.Log("tilemap is " + (tilemap == null ? "NULL" : "OK"));

        if (clustering == null || tilemap == null)
            return;

        Debug.Log("clusters is " +
            (clustering.clusters == null ? "NULL" : clustering.clusters.Count.ToString()));

        BuildNodes();
        BuildConnections();
    }


    void BuildNodes()
    {
        nodes.Clear();

        for (int i = 0; i < clustering.clusters.Count; i++)
        {
            var cluster = clustering.clusters[i];

            Bounds bounds = CalculateBounds(cluster);
            Vector3 center = bounds.center;

            NavNode node = new NavNode(i, center, bounds);
            nodes.Add(node);
        }

        Debug.Log($"Built {nodes.Count} NavNodes");
    }

    Bounds CalculateBounds(Vector3Int[] cluster)
    {
        Vector3 min = tilemap.CellToWorld(cluster[0]);
        Vector3 max = min;

        foreach (var cell in cluster)
        {
            Vector3 world = tilemap.CellToWorld(cell);
            min = Vector3.Min(min, world);
            max = Vector3.Max(max, world);
        }

        Bounds b = new Bounds();
        b.SetMinMax(min, max + tilemap.cellSize);
        return b;
    }

    void BuildConnections()
    {
        // for test, change to enemy info in real game
        float maxHorizontal = 4.5f;
        float maxVertical = 4.5f;

        foreach (var a in nodes)
        {
            a.neighbors.Clear();

            foreach (var b in nodes)
            {
                if (a == b) continue;

                Vector3 delta = b.center - a.center;

                // horizontal distance
                if (Mathf.Abs(delta.x) > maxHorizontal)
                    continue;

                // vertical distance
                if (delta.y > maxVertical || delta.y < -maxVertical)
                    continue;

                a.neighbors.Add(b);
            }
        }

        Debug.Log("Connections built");
    }


    void OnDrawGizmos()
{
    if (nodes == null) return;

    Gizmos.color = Color.cyan;
    foreach (var n in nodes)
    {
        Gizmos.DrawSphere(n.center, 0.15f);

        if (n.neighbors == null) continue;

        Gizmos.color = Color.yellow;
        foreach (var nei in n.neighbors)
        {
            Gizmos.DrawLine(n.center, nei.center);
        }

        Gizmos.color = Color.cyan;
    }
}

}
