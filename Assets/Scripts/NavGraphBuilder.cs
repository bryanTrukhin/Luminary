using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NavGraphBuilder : MonoBehaviour
{
    [SerializeField] GameObject gameManager;
    public PlatformClustering clustering;
    public Tilemap tilemap;

    public List<NavNode> nodes = new List<NavNode>();

    public Transform enemy;
    public Transform target;

    public List<TileNode> debugTilePath;

    private void Start()
    {
        if (clustering)
        {
            Initialize(clustering);
        }
    }

    public void Initialize(PlatformClustering clusteringRef)
    {
        clustering = clusteringRef;
        tilemap = clustering.tilemap;

        GameObject enemyObj = GameObject.FindWithTag("Enemy"); // Ensure your enemy has this tag
        if (enemyObj != null) enemy = enemyObj.transform;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) target = playerObj.transform;

        if (clustering == null || tilemap == null) return;

        BuildNodes();
        BuildConnections();
    }

    void Update()
    {
        if (enemy == null || target == null || clustering == null) return;

        var enemyCluster = GetClusterOfPosition(enemy.position);
        var targetCluster = GetClusterOfPosition(target.position);

        if (enemyCluster == null || targetCluster == null)
        {
            //Debug.Log("Cluster missing");
            return;
        }

        if (enemyCluster == targetCluster)
        {
            TileNode startTile = GetClosestTile(enemy.position, enemyCluster);
            TileNode goalTile = GetClosestTile(target.position, targetCluster);

            debugTilePath = TileAStar.FindPath(
                 startTile,
                 goalTile,
                 enemyCluster.tileNodes
            );
        }
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

    //void Update()
    //{
    //    var enemyCluster = GetClusterOfPosition(enemy.position);
    //    var targetCluster = GetClusterOfPosition(target.position);

    //    if (enemyCluster == null || targetCluster == null)
    //    {
    //        Debug.Log("Cluster missing");
    //        return;
    //    }

    //    if (enemyCluster == targetCluster)
    //    {
    //        Debug.Log("Same Cluster ?? Tile A*");

    //        TileNode startTile = GetClosestTile(enemy.position, enemyCluster);
    //        TileNode goalTile = GetClosestTile(target.position, targetCluster);

    //        debugTilePath = TileAStar.FindPath(
    //             startTile,
    //             goalTile,
    //             enemyCluster.tileNodes
    //        );

    //        Debug.Log(debugTilePath == null
    //                ? "No tile path"
    //                : $"Tile path length: {debugTilePath.Count}");
    //    }
    //    else
    //    {
    //        Debug.Log("Different Cluster ?? Nav A* (later)");
    //    }
    //}

    TileNode GetClosestTile(Vector3 worldPos, PlatformCluster cluster)
    {
        TileNode best = null;
        float bestDist = float.MaxValue;

        foreach (var node in cluster.tileNodes)
        {
            if (node.neighbors.Count == 0)
                continue;

            float d = Vector2.Distance(worldPos, node.worldPos);

            if (d < bestDist)
            {
                bestDist = d;
                best = node;
            }
        }
        return best;
    }

    PlatformCluster GetClusterOfPosition(Vector3 worldPos)
    {
        Vector3 probePos = worldPos + Vector3.down * 1.1f;
        Vector3Int cell = tilemap.WorldToCell(probePos);

        //Debug.Log($"[ClusterCheck] world={worldPos} probe={probePos} cell={cell} hasTile={tilemap.HasTile(cell)}");

        foreach (var cluster in clustering.clusters)
        {
            if (cluster.tiles.Contains(cell))
            {
                //Debug.Log($"Found cluster {cluster.id}");
                return cluster;
            }
        }

        //Debug.Log("Cluster missing");
        return null;
    }

    PlatformCluster GetClusterByClosestTile(Vector3 worldPos)
    {
        float bestDist = float.MaxValue;
        PlatformCluster best = null;

        foreach (var cluster in clustering.clusters)
        {
            foreach (var node in cluster.tileNodes)
            {
                float d = Vector2.Distance(worldPos, node.worldPos);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = cluster;
                }
            }
        }
        return best;
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
        if (debugTilePath != null)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < debugTilePath.Count - 1; i++)
            {
                Gizmos.DrawLine(
                    debugTilePath[i].worldPos,
                    debugTilePath[i + 1].worldPos
                );
            }
        }

    }
    public List<TileNode> GetTilePath(Vector3 startWorldPos, Vector3 targetWorldPos)
    {
        PlatformCluster startCluster = GetClusterOfPosition(startWorldPos);
        PlatformCluster targetCluster = GetClusterOfPosition(targetWorldPos);

        if (startCluster == null || targetCluster == null || startCluster != targetCluster)
            return null;

        TileNode startTile = GetClosestTile(startWorldPos, startCluster);
        TileNode goalTile = GetClosestTile(targetWorldPos, targetCluster);

        return TileAStar.FindPath(startTile, goalTile, startCluster.tileNodes);
    }

}
