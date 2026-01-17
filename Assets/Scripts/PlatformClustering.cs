using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlatformClustering : MonoBehaviour
{
    [SerializeField] public Tilemap tilemap;
    public List<Vector3Int[]> clusters;

    // Start is called before the first frame update
    void Awake()
    {
        clusters = FindAllClusters(tilemap);
        Debug.Log("PlatformClustering clusters: " + clusters.Count);

        clusters = FindAllClusters(tilemap);
    }


    // Update is called once per frame
    void Update()
    {
        //not needed right now
    }

    public List<Vector3Int[]> FindAllClusters(Tilemap tilemap)
    {
        // This holds finished clusters
        List<Vector3Int[]> allClusters = new List<Vector3Int[]>();

        // This holds visted cells
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        BoundsInt bounds = tilemap.cellBounds;

        //Scanning all possible cells (outer loop will create seed points, and the BFS will explore the seed points to make a cluster)
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos) || visited.Contains(pos))
            {
                continue;
            }

            //BFS logic
            List<Vector3Int> currentCluster = new List<Vector3Int>();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();

            queue.Enqueue(pos);
            visited.Add(pos);

            while (queue.Count > 0)
            {
                Vector3Int current = queue.Dequeue();
                currentCluster.Add(current);

                // Check the 4 possible neighbors
                Vector3Int[] neighbors = {
                    current + Vector3Int.up, current + Vector3Int.down,
                    current + Vector3Int.left, current + Vector3Int.right,
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
            allClusters.Add(currentCluster.ToArray());
        }
        return allClusters;
    }

    //Drawing the clusters for visualization
    void OnDrawGizmos()
    {
        if (clusters == null || clusters.Count == 0 || tilemap == null) return;

        for (int i = 0; i < clusters.Count; i++)
        {
            Vector3Int[] currentCluster = clusters[i];

            Vector2Int min = new Vector2Int(currentCluster[0].x, currentCluster[0].y);
            Vector2Int max = new Vector2Int(currentCluster[0].x, currentCluster[0].y);
            foreach (Vector3Int pos in currentCluster)
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
        }
    }
}
