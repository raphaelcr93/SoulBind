public class GridTileType
{
    //public GameObject tileVisualPrefab;
    public string pathToPrefab {get; private set;}
    public float movementCost {get; private set;}
    public bool isWalkable {get; private set;}
    public float colorR {get; private set;}
    public float colorG  {get; private set;}
    public float colorB  {get; private set;}

    public GridTileType(string pathToPrefab, float movementCost, bool isWalkable, float colorR, float colorG, float colorB)
    {
        this.pathToPrefab = pathToPrefab;
        this.movementCost = movementCost;
        this.isWalkable = isWalkable;
        this.colorR = colorR;
        this.colorG = colorG;
        this.colorB = colorB;

    }

    public enum GridTileTypeName
    {
        Normal,
        Mountain,
        Swamp
    }

}

