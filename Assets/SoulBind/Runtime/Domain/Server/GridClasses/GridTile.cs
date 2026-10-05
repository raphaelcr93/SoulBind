public class GridTile 
{
    public GridTileType tileType {get; private set;}
    public GridPosition tilePosition {get; private set;}
    public UnitServer unitInTile {get; private set;}
    public float currentMovementCost {get; private set;}
    public bool currentIsWalkable  {get; private set;}

    public GridTile (GridTileType type, GridPosition position) {
        this.tilePosition = position;
        SetTileType(type);
    }

    public void SetTileUnit (UnitServer unit) {
        this.unitInTile = unit;
    }

    public void SetTileType (GridTileType type) {
        this.tileType = type;
        currentMovementCost = type.movementCost;
        currentIsWalkable = type.isWalkable;
    }

}