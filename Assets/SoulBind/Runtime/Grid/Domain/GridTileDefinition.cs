namespace SoulBind.Grid;

public sealed class GridTileDefinition
{
    #region Properties

    public string Id { get; }
    public int MovementCost { get; }
    public bool IsWalkable { get; }

    #endregion

    #region Construction

    public GridTileDefinition(string id, int movementCost, bool isWalkable)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new System.ArgumentException("A tile definition requires an id.", nameof(id));
        if (movementCost < 1)
            throw new System.ArgumentOutOfRangeException(nameof(movementCost));

        Id = id;
        MovementCost = movementCost;
        IsWalkable = isWalkable;
    }

    #endregion
}
