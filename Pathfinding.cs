using System.Numerics;
using Ation.GameWorld;
using System.Collections.Generic;
using System.Linq;

namespace Ation.Common;

public class AStarNode
{
    public int X, Y;
    public AStarNode? Parent;
    public float GCost, HCost;
    public float FCost => GCost + HCost;

    public AStarNode(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj) => obj is AStarNode n && n.X == X && n.Y == Y;
    public override int GetHashCode() => HashCode.Combine(X, Y);
}

public static class AStarPathfinder
{
    // These are public for debug visualization (optional)
    public static HashSet<AStarNode> DebugOpenSet = new();
    public static HashSet<AStarNode> DebugClosedSet = new();
    public static List<Vector2> DebugPath = new();

    public static List<Vector2> FindPath(World world, Vector2 start, Vector2 end)
    {
        // Reset debug data
        DebugOpenSet.Clear();
        DebugClosedSet.Clear();
        DebugPath.Clear();

        // Open = frontier (to be explored)
        // Closed = already visited
        var open = new List<AStarNode>();
        var closed = new HashSet<AStarNode>();

        // Round coordinates to grid cells
        var startNode = new AStarNode((int)start.X, (int)start.Y);
        var endNode = new AStarNode((int)end.X, (int)end.Y);

        open.Add(startNode);
        DebugOpenSet.Add(startNode);

        while (open.Count > 0)
        {
            // Select node with lowest F-cost (G + H)
            var current = open.OrderBy(n => n.FCost).First();
            open.Remove(current);
            DebugOpenSet.Remove(current);
            closed.Add(current);
            DebugClosedSet.Add(current);

            // Found goal
            if (current.X == endNode.X && current.Y == endNode.Y)
            {
                var path = ReconstructPath(current);
                DebugPath = path;
                return path;
            }

            // Expand neighbors
            foreach (var neighbor in GetNeighbors(current, world))
            {
                // Skip blocked or already visited nodes
                if (closed.Contains(neighbor) || world.IsCollidableAt(neighbor.X, neighbor.Y))
                    continue;

                float tentativeG = current.GCost + 1; // cost from start

                var existing = open.FirstOrDefault(n => n.X == neighbor.X && n.Y == neighbor.Y);
                if (existing == null)
                {
                    // New node discovered
                    neighbor.GCost = tentativeG;
                    neighbor.HCost = Heuristic(neighbor, endNode);
                    neighbor.Parent = current;
                    open.Add(neighbor);
                    DebugOpenSet.Add(neighbor);
                }
                else if (tentativeG < existing.GCost)
                {
                    // Found a cheaper path to existing node
                    existing.GCost = tentativeG;
                    existing.Parent = current;
                }
            }
        }

        // No path found
        return new List<Vector2>();
    }

    // Heuristic = estimated distance to goal (Manhattan distance for grid)
    private static float Heuristic(AStarNode a, AStarNode b)
    {
        return MathF.Abs(a.X - b.X) + MathF.Abs(a.Y - b.Y);
    }

    // Returns walkable 4-directional neighbor nodes
    private static List<AStarNode> GetNeighbors(AStarNode node, World world)
    {
        var neighbors = new List<AStarNode>();
        int x = node.X;
        int y = node.Y;

        bool grounded = world.IsCollidableAt(x, y + 1);

        // === 1. Walk left/right if grounded ===
        if (grounded)
        {
            for (int dx = -1; dx <= 1; dx += 2)
            {
                int nx = x + dx;
                int ny = y;

                if (!world.IsCollidableAt(nx, ny) && world.IsCollidableAt(nx, ny + 1))
                    neighbors.Add(new AStarNode(nx, ny));
            }
        }

        // === 2. Fall down if no ground ===
        if (!grounded)
        {
            for (int dy = 1; dy <= 4; dy++)
            {
                int ny = y + dy;
                if (world.IsCollidableAt(x, ny)) break;
                neighbors.Add(new AStarNode(x, ny));
            }
        }

        // === 3. Jump targets (platforms above)
        if (grounded)
        {
            for (int dy = 1; dy <= 40; dy++) // jump height
            {
                int ny = y - dy;

                for (int dx = -3; dx <= 10; dx++) // horizontal range
                {
                    int nx = x + dx;
                    if (world.IsValidCell(nx, ny) &&
                        !world.IsCollidableAt(nx, ny) &&
                        world.IsCollidableAt(nx, ny + 1))
                    {
                        neighbors.Add(new AStarNode(nx, ny));
                    }
                }
            }
        }

        return neighbors;
    }


    // Reconstructs path from goal → start by walking up the parent chain
    private static List<Vector2> ReconstructPath(AStarNode node)
    {
        var path = new List<Vector2>();
        while (node != null)
        {
            path.Add(new Vector2(node.X, node.Y));
            node = node.Parent;
        }

        path.Reverse(); // we built it backwards
        return path;
    }
}
