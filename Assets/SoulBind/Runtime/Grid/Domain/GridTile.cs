namespace SoulBind.Grid;

public sealed class GridTile
{
    #region Properties

    public GridPosition Position { get; }
    public GridTileDefinition Definition { get; private set; }
    public string OccupantId { get; private set; }
    public bool IsOccupied => OccupantId != null;
    public bool IsWalkable => Definition.IsWalkable && !IsOccupied;

    #endregion

    #region Construction

    public GridTile(GridPosition position, GridTileDefinition definition)
    {
        Position = position;
        Definition = definition ?? throw new System.ArgumentNullException(nameof(definition));
    }

    #endregion

    #region Runtime State

    public void ChangeDefinition(GridTileDefinition definition) =>
        Definition = definition ?? throw new System.ArgumentNullException(nameof(definition));

    public bool TrySetOccupant(string occupantId)
    {
        if (string.IsNullOrWhiteSpace(occupantId) || IsOccupied)
            return false;
        OccupantId = occupantId;
        return true;
    }

    public void ClearOccupant() => OccupantId = null;

    #endregion
}
