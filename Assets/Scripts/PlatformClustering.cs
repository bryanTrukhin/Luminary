using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlatformClustering : MonoBehaviour
{
    public Tilemap tilemap;
    public List<PlatformCluster> clusters;

    void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();

        // 1. ALWAYS assign references first!
        if (tilemap == null)
        {
            // Try to find the tilemap on this object or its children
            tilemap = GetComponent<Tilemap>();
            if (tilemap == null) tilemap = GetComponentInChildren<Tilemap>();
        }

        // 2. Double-check to prevent the crash if it's STILL null
        if (tilemap == null)
        {
            Debug.LogError($"PlatformClustering on {gameObject.name} can't find a Tilemap!");
            return;
        }

        // 3. Now it is safe to run the logic
        clusters = FindAllClusters(tilemap);

        foreach (var cluster in clusters)
        {
            BuildTileNodes(cluster);
        }

        FindClusterExits();

        Debug.Log("PlatformClustering clusters: " + clusters.Count);
    }

    void Update()
    {
        // not needed right now
    }

    public List<PlatformCluster> FindAllClusters(Tilemap tilemap)
    {
        List<PlatformCluster> allClusters = new List<PlatformCluster>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        BoundsInt bounds = tilemap.cellBounds;

        int clusterId = 0;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos) || visited.Contains(pos))
                continue;

            PlatformCluster cluster = new PlatformCluster();
            cluster.id = clusterId++;

            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            queue.Enqueue(pos);
            visited.Add(pos);

            Vector3Int sum = Vector3Int.zero;

            while (queue.Count > 0)
            {
                Vector3Int current = queue.Dequeue();
                cluster.tiles.Add(current);
                sum += current;

                Vector3Int[] neighbors = {
                    current + Vector3Int.up,
                    current + Vector3Int.down,
                    current + Vector3Int.left,
                    current + Vector3Int.right,
                    current + Vector3Int.right + Vector3Int.up,
                    current + Vector3Int.left + Vector3Int.up,
                    current + Vector3Int.right + Vector3Int.down,
                    current + Vector3Int.left + Vector3Int.down
                };

                foreach (Vector3Int n in neighbors)
                {
                    if (tilemap.HasTile(n) && !visited.Contains(n))
                    {
                        visited.Add(n);
                        queue.Enqueue(n);
                    }
                }
            }

            Vector3 avgCell = (Vector3)sum / cluster.tiles.Count;
            cluster.center = tilemap.CellToWorld(Vector3Int.RoundToInt(avgCell));
            allClusters.Add(cluster);
        }

        return allClusters;
    }

    void BuildTileNodes(PlatformCluster cluster)
    {
        cluster.tileNodes.Clear();
        cluster.tileNodeMap.Clear();

        foreach (Vector3Int cell in cluster.tiles)
        {
            if (tilemap.HasTile(cell) && !tilemap.HasTile(cell + Vector3Int.up))
            {
                TileNode node = new TileNode();
                node.cellPos = cell;
                node.worldPos = tilemap.CellToWorld(cell) + tilemap.cellSize / 2f;

                cluster.tileNodes.Add(node);
                cluster.tileNodeMap[cell] = node;
            }
        }

        BuildTileNeighbors(cluster);

        Debug.Log($"Cluster {cluster.id} surface nodes: {cluster.tileNodes.Count}");
    }

    void FindClusterExits()
    {
        float maxJumpDistance = 6f;

        foreach (var a in clusters)
        {
            foreach (var node in a.tileNodes)
            {
                foreach (var b in clusters)
                {
                    if (a == b) continue;

                    foreach (var otherNode in b.tileNodes)
                    {
                        float dist = Vector2.Distance(node.worldPos, otherNode.worldPos);

                        if (dist < maxJumpDistance)
                        {
                            a.exits.Add(node);
                            break;
                        }
                    }
                }
            }
        }
    }

    void BuildTileNeighbors(PlatformCluster cluster)
    {
        foreach (var node in cluster.tileNodes)
        {
            Vector3Int c = node.cellPos;

            Vector3Int[] candidates =
            {
                Vector3Int.left,
                Vector3Int.right,
                Vector3Int.left + Vector3Int.up,
                Vector3Int.right + Vector3Int.up,
                Vector3Int.left + Vector3Int.down,
                Vector3Int.right + Vector3Int.down
            };

            foreach (var offset in candidates)
            {
                Vector3Int target = c + offset;

                if (cluster.tileNodeMap.TryGetValue(target, out TileNode neighbor))
                {
                    int heightDiff = Mathf.Abs(target.y - c.y);
                    if (heightDiff <= 1)
                    {
                        node.neighbors.Add(neighbor);
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (clusters == null || clusters.Count == 0 || tilemap == null) return;

        for (int i = 0; i < clusters.Count; i++)
        {
            PlatformCluster cluster = clusters[i];
            if (cluster.tiles.Count == 0) continue;

            Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
            foreach (Vector3Int cell in cluster.tiles)
            {
                Vector3 worldPos = tilemap.CellToWorld(cell) + tilemap.cellSize / 2f;
                Gizmos.DrawCube(worldPos, tilemap.cellSize);
            }

            Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);

            foreach (Vector3Int pos in cluster.tiles)
            {
                if (pos.x < min.x) min.x = pos.x;
                if (pos.y < min.y) min.y = pos.y;
                if (pos.x > max.x) max.x = pos.x;
                if (pos.y > max.y) max.y = pos.y;
            }

            Vector3 worldMin = tilemap.CellToWorld(new Vector3Int(min.x, min.y, 0));
            Vector3 worldMax = tilemap.CellToWorld(new Vector3Int(max.x + 1, max.y + 1, 0));

            Vector3 size = worldMax - worldMin;
            Vector3 center = worldMin + (size / 2f);

            float airBias = 1.0f;
            size.y += airBias;
            center.y += airBias / 2f;

            Gizmos.color = Color.HSVToRGB((i * 0.2f) % 1f, 0.8f, 1f);
            Gizmos.DrawWireCube(center, size);

            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.1f);
            Gizmos.DrawCube(center, size);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(cluster.center, 0.15f);

            Gizmos.color = Color.blue;
            foreach (var node in cluster.tileNodes)
            {
                Gizmos.DrawSphere(node.worldPos, 0.08f);
            }

            Gizmos.color = Color.cyan;
            foreach (var node in cluster.tileNodes)
            {
                foreach (var nei in node.neighbors)
                {
                    Gizmos.DrawLine(node.worldPos, nei.worldPos);
                }
            }
        }
    }
}