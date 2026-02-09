using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NavGraphBuilder : MonoBehaviour
{
    public PlatformClustering clustering;
    public Tilemap tilemap;

    public List<NavNode> nodes = new List<NavNode>();

    public Transform enemy;
    public Transform target;

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

    Bounds CalculateBounds(PlatformCluster cluster)
    {
        bool first = true;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;

        foreach (Vector3Int cell in cluster.tiles)
        {
            Vector3 world = tilemap.CellToWorld(cell);

            if (first)
            {
                min = world;
                max = world + tilemap.cellSize;
                first = false;
            }
            else
            {
                min = Vector3.Min(min, world);
                max = Vector3.Max(max, world + tilemap.cellSize);
            }
        }

        Bounds b = new Bounds();
        b.SetMinMax(min, max);
        return b;
    }


    void BuildConnections()
    {
        // for test, change to enemy info in real game
        float maxHorizontal = 6f;
        float maxVertical = 6f;

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

    NavNode GetClosestNode(Vector3 pos)
    {
        NavNode best = null;
        float bestDist = float.MaxValue;

        foreach (var n in nodes)
        {
            float d = Vector2.Distance(pos, n.center);
            if (d < bestDist)
            {
                bestDist = d;
                best = n;
            }
        }
        return best;
    }

    List<NavNode> FindPath(NavNode start, NavNode goal)
    {
        var open = new List<NavNode>();
        var closed = new HashSet<NavNode>();

        foreach (var n in nodes)
        {
            n.gCost = float.MaxValue;
            n.parent = null;
        }

        start.gCost = 0;
        start.hCost = Vector2.Distance(start.center, goal.center);
        open.Add(start);

        while (open.Count > 0)
        {
            NavNode current = open[0];
            foreach (var n in open)
            {
                if (n.fCost < current.fCost)
                    current = n;
            }

            if (current == goal)
                return ReconstructPath(goal);

            open.Remove(current);
            closed.Add(current);

            foreach (var nei in current.neighbors)
            {
                if (closed.Contains(nei))
                    continue;

                float tentativeG =
                    current.gCost + Vector2.Distance(current.center, nei.center);

                if (tentativeG < nei.gCost)
                {
                    nei.parent = current;
                    nei.gCost = tentativeG;
                    nei.hCost = Vector2.Distance(nei.center, goal.center);

                    if (!open.Contains(nei))
                        open.Add(nei);
                }
            }
        }

        return null;
    }

    List<NavNode> ReconstructPath(NavNode end)
    {
        var path = new List<NavNode>();
        NavNode cur = end;

        while (cur != null)
        {
            path.Add(cur);
            cur = cur.parent;
        }

        path.Reverse();
        return path;
    }

    List<NavNode> debugPath;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            NavNode start = GetClosestNode(enemy.position);
            NavNode goal = GetClosestNode(target.position);

            if (start == null || goal == null)
            {
                Debug.Log("Start or goal node missing");
                return;
            }

            debugPath = FindPath(start, goal);

            Debug.Log(debugPath == null
                ? "No path found"
                : $"Path length: {debugPath.Count}");
        }
    }


    void OnDrawGizmos()
    {
        if (nodes == null) return;

        Gizmos.color = Color.cyan;
        foreach (var n in nodes)
        {
            Gizmos.DrawSphere(n.center, 0.15f);
            {
                if (n.neighbors == null) continue;
            }
            Gizmos.color = Color.yellow;
            foreach (var nei in n.neighbors)
            {
                Gizmos.DrawLine(n.center, nei.center);
            }

            Gizmos.color = Color.cyan;
        }
        if (debugPath != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < debugPath.Count - 1; i++)
            {
                Gizmos.DrawLine(
                    debugPath[i].center,
                    debugPath[i + 1].center
                );
            }
        }

    }

}
