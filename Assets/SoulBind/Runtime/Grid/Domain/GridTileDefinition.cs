namespace SoulBind.Grid;

public sealed class GridTileDefinition
{
    public string Id { get; }
    public int MovementCost { get; }
    public bool IsWalkable { get; }

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
}
