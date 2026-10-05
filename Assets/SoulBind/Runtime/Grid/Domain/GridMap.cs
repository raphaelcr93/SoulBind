using System;
using System.Collections.Generic;

namespace SoulBind.Grid
{

public sealed class GridMap
{
    #region Fields and Properties

    private readonly GridTile[,] tiles;

    public int Width { get; }
    public int Height { get; }

    #endregion

    #region Construction

    public GridMap(int width, int height, GridTileDefinition defaultDefinition)
    {
        if (width < 1) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 1) throw new ArgumentOutOfRangeException(nameof(height));
        if (defaultDefinition == null) throw new ArgumentNullException(nameof(defaultDefinition));

        Width = width;
        Height = height;
        tiles = new GridTile[width, height];
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            tiles[x, y] = new GridTile(new GridPosition(x, y), defaultDefinition);
    }

    #endregion

    #region Queries

    public bool Contains(GridPosition position) =>
        position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;

    public GridTile Get(GridPosition position)
    {
        if (!Contains(position)) throw new ArgumentOutOfRangeException(nameof(position));
        return tiles[position.X, position.Y];
    }

    public bool TryGet(GridPosition position, out GridTile tile)
    {
        tile = Contains(position) ? tiles[position.X, position.Y] : null;
        return tile != null;
    }

    public IEnumerable<GridPosition> GetNeighbours(GridPosition position)
    {
        var offsets = new[] { new GridPosition(1, 0), new GridPosition(-1, 0), new GridPosition(0, 1), new GridPosition(0, -1) };
        foreach (var offset in offsets)
        {
            var neighbour = new GridPosition(position.X + offset.X, position.Y + offset.Y);
            if (Contains(neighbour)) yield return neighbour;
        }
    }

    #endregion
}
}
