using System.Collections.Generic;

namespace SoulBind.Grid;

public interface IGridPathfinder
{
    IReadOnlyList<GridPosition> FindPath(GridMap map, GridPosition start, GridPosition goal);
}

public sealed class DijkstraGridPathfinder : IGridPathfinder
{
    public IReadOnlyList<GridPosition> FindPath(GridMap map, GridPosition start, GridPosition goal)
    {
        if (map == null || !map.Contains(start) || !map.Contains(goal)) return System.Array.Empty<GridPosition>();
        if (!map.Get(start).IsWalkable || !map.Get(goal).IsWalkable) return System.Array.Empty<GridPosition>();

        var frontier = new List<GridPosition> { start };
        var cost = new Dictionary<GridPosition, int> { [start] = 0 };
        var previous = new Dictionary<GridPosition, GridPosition>();

        while (frontier.Count > 0)
        {
            var current = frontier[0];
            frontier.RemoveAt(0);
            if (current == goal) break;

            foreach (var next in map.GetNeighbours(current))
            {
                var tile = map.Get(next);
                if (!tile.IsWalkable) continue;
                var nextCost = cost[current] + tile.Definition.MovementCost;
                if (cost.ContainsKey(next) && cost[next] <= nextCost) continue;
                cost[next] = nextCost;
                previous[next] = current;
                frontier.Add(next);
            }
        }

        if (start != goal && !previous.ContainsKey(goal)) return System.Array.Empty<GridPosition>();
        var path = new List<GridPosition> { goal };
        while (path[path.Count - 1] != start) path.Add(previous[path[path.Count - 1]]);
        path.Reverse();
        return path;
    }
}
