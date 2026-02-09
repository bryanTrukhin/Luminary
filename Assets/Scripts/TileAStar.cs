using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileAStar
{
    public static List<TileNode> FindPath(
        TileNode start,
        TileNode goal)
    {
        if (start == null || goal == null)
            return null;

        var open = new List<TileNode>();
        var closed = new HashSet<TileNode>();

        ResetNodes(start, goal);

        start.gCost = 0;
        start.hCost = Vector2.Distance(start.worldPos, goal.worldPos);
        start.parent = null;

        open.Add(start);

        while (open.Count > 0)
        {
            TileNode current = open[0];
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
                    current.gCost +
                    Vector2.Distance(current.worldPos, nei.worldPos);

                if (!open.Contains(nei) || tentativeG < nei.gCost)
                {
                    nei.parent = current;
                    nei.gCost = tentativeG;
                    nei.hCost = Vector2.Distance(nei.worldPos, goal.worldPos);

                    if (!open.Contains(nei))
                        open.Add(nei);
                }
            }
        }

        return null;
    }

    static void ResetNodes(TileNode start, TileNode goal)
    {
        start.gCost = float.MaxValue;
        start.hCost = 0;
        start.parent = null;

        goal.gCost = float.MaxValue;
        goal.hCost = 0;
        goal.parent = null;
    }

    static List<TileNode> ReconstructPath(TileNode end)
    {
        var path = new List<TileNode>();
        TileNode cur = end;

        while (cur != null)
        {
            path.Add(cur);
            cur = cur.parent;
        }

        path.Reverse();
        return path;
    }
}
