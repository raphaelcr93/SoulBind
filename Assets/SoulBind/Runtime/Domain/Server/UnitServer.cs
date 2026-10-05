public class UnitServer : ITurnActor
{

    public GridPosition unitPosition { get; private set; }
    public string prefabPath { get; private set; }
    public float prefabScale;
    public string playerToken { get; private set; }
    public UnitStats unitStats { get; private set; }

    public UnitServer(GridPosition unitPosition, string pathToPrefab, float unitPrefabScale, string playerToken, UnitStats unitStats ) {
        this.unitPosition = unitPosition;
        this.prefabPath = pathToPrefab;
        this.prefabScale = unitPrefabScale;
        this.playerToken = playerToken;
        this.unitStats = unitStats;
    }

    public bool Unit_IsDead()
    {
        return unitStats.hp <= 0;
    }

    public void Unit_SetPosition(GridPosition position)
    {
        this.unitPosition = position;
    }

    public void Unit_TakeDamage(int damage)
    {
        unitStats.hp -= damage;
    }

    public void OnTurnStart()
    {
        //throw new System.NotImplementedException();
    }

    public void OnTurnEnd()
    {
        //throw new System.NotImplementedException();
    }


}
