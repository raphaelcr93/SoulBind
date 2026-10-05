using System.Linq;
using NUnit.Framework;
using SoulBind.Grid;

namespace SoulBind.Tests.Grid
{

public sealed class GridFoundationTests
{
    #region Test Fixtures

    private static GridTileDefinition WalkableTile => new("grass", 1, true);

    #endregion

    #region Position Tests

    [Test]
    public void PositionsWithSameCoordinatesAreEqual()
    {
        Assert.That(new GridPosition(2, 3), Is.EqualTo(new GridPosition(2, 3)));
    }

    #endregion

    #region Map Tests

    [Test]
    public void MapRejectsPositionsOutsideItsBounds()
    {
        var map = new GridMap(3, 2, WalkableTile);
        Assert.That(map.Contains(new GridPosition(-1, 0)), Is.False);
        Assert.That(map.Contains(new GridPosition(2, 1)), Is.True);
        Assert.That(map.Contains(new GridPosition(3, 1)), Is.False);
    }

    [Test]
    public void OccupiedTileIsNotWalkable()
    {
        var tile = new GridMap(1, 1, WalkableTile).Get(new GridPosition(0, 0));
        Assert.That(tile.TrySetOccupant("unit-1"), Is.True);
        Assert.That(tile.IsWalkable, Is.False);
    }

    #endregion

    #region Pathfinding Tests

    [Test]
    public void PathfinderReturnsAPathBetweenOpenTiles()
    {
        var map = new GridMap(3, 1, WalkableTile);
        var path = new DijkstraGridPathfinder().FindPath(map, new GridPosition(0, 0), new GridPosition(2, 0));
        Assert.That(path.ToArray(), Is.EqualTo(new[] { new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(2, 0) }));
    }

    [Test]
    public void PathfinderReturnsNoPathWhenGoalIsBlocked()
    {
        var map = new GridMap(2, 1, WalkableTile);
        map.Get(new GridPosition(1, 0)).TrySetOccupant("unit-1");
        Assert.That(new DijkstraGridPathfinder().FindPath(map, new GridPosition(0, 0), new GridPosition(1, 0)), Is.Empty);
    }

    [Test]
    public void PathfinderRoutesAroundBlockedTile()
    {
        var map = new GridMap(3, 2, WalkableTile);
        map.Get(new GridPosition(1, 0)).ChangeDefinition(new GridTileDefinition("wall", 1, false));
        var path = new DijkstraGridPathfinder().FindPath(map, new GridPosition(0, 0), new GridPosition(2, 0));
        Assert.That(path.Any(position => position == new GridPosition(1, 0)), Is.False);
        Assert.That(path[path.Count - 1], Is.EqualTo(new GridPosition(2, 0)));
    }

    #endregion
}
}
