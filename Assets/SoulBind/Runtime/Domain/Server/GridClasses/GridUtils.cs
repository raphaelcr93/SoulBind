using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public static class GridUtils
{

    public static Vector3 GridCoordToWorldCord(int x, int y)
    {
        GridPosition gridPosition = new GridPosition(x, y);

        return GridCoordToWorldCordWithHeightOffset(gridPosition, 0);
    }

    public static Vector3 GridCoordToWorldCord(GridPosition gridPosition)
    {
        return GridCoordToWorldCordWithHeightOffset(gridPosition, 0);
    }

    public static Vector3 GridCoordToWorldCordWithHeightOffset(GridPosition gridPosition, float heightOffset)
    {
        return new Vector3(gridPosition.x, gridPosition.y, heightOffset);
    }

    public static bool IsSamePosition(GridPosition gridPosition1, GridPosition gridPosition2)
    {
        return gridPosition1.x == gridPosition2.x && gridPosition1.y == gridPosition2.y;
    }

    public static UnitServer GetServerUnitFromITurnActor(ITurnActor turnActor)
    {
        if (turnActor is UnitServer)
        {
            UnitServer instance = turnActor as UnitServer;
            return instance;
        }
        return null;
    }

    public static Vector2 DirectionBetweenGridPositions(GridPosition sourcePosition, GridPosition targetPosition)
    {
        Vector2 sourceVector = new Vector2(sourcePosition.x, sourcePosition.y);
        Vector2 targetVector = new Vector2(targetPosition.x, targetPosition.y);
        return (targetVector - sourceVector).normalized;
    }

    public static bool CheckIfTileExistInServerMap(int x, int y, GridTile[,] serverMap)
    {
        return serverMap[x, y] != null;
    }

    public static bool CheckIfTileExistInServerMap(GridPosition tilePosition, GridTile[,] serverMap)
    {
        return CheckIfTileExistInServerMap(tilePosition.x, tilePosition.y, serverMap);
    }

    public static GridTile GetGridTileAtPosition(GridPosition gridPosition, GridTile[,] serverMap)
    {
        return serverMap[gridPosition.x, gridPosition.y];
    }

    public static UnitServer GetUnitServerAtPosition(GridPosition gridPosition, GridTile[,] serverMap)
    {
        GridTile gridTile = GetGridTileAtPosition(gridPosition, serverMap);
        return gridTile.unitInTile;
    }

    public static Node GetNodeAtPosition(GridPosition gridPosition, Node[,] graphMap) {
        return graphMap[gridPosition.x, gridPosition.y];
    }

}
