using System;

namespace SoulBind.Grid
{

public readonly struct GridPosition : IEquatable<GridPosition>
{
    #region Properties

    public int X { get; }
    public int Y { get; }

    #endregion

    #region Construction

    public GridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    #endregion

    #region Equality and Formatting

    public bool Equals(GridPosition other) => X == other.X && Y == other.Y;
    public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public static bool operator ==(GridPosition left, GridPosition right) => left.Equals(right);
    public static bool operator !=(GridPosition left, GridPosition right) => !left.Equals(right);
    public override string ToString() => $"({X}, {Y})";

    #endregion
}
}
