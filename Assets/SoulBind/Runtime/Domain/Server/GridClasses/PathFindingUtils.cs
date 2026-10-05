
using System.Collections.Generic;
using UnityEngine;


public class PathFindingUtils
{

    public enum PathCostType
    {
        Movement,
        Attack
    }

    public enum PathFindingAlgorithm
    {
        Dijkstra
    }

    private static HashSet<Node> GetPossibleTilesBasedInCost(PathCostType costType, GridPosition source, int availablePoints, GridServerInfo gridInfo)
    {
        float[,] cost = new float[gridInfo.graphMap.GetLength(0), gridInfo.graphMap.GetLength(1)];
        //Nodes to iterate and get the cost
        HashSet<Node> nodesToIterate = new HashSet<Node>();
        //Temp var to store the nodes that our unit can reach
        HashSet<Node> tempReachableNodes = new HashSet<Node>();
        //Final hashSet of all reachable nodes
        HashSet<Node> finalReachableNodes = new HashSet<Node>();
        Node unitInitialNode = gridInfo.graphMap[source.x, source.y];
        float tempCost;
        float currCost;

        ///Set-up the initial costs for the neighbouring nodes
        //finalReachableNodes.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.neighbours)
        {
            cost[n.gridPosition.x, n.gridPosition.y] = PathCostToTile(costType, source, n.gridPosition, gridInfo);
            if (availablePoints - cost[n.gridPosition.x, n.gridPosition.y] >= 0)
            {
                nodesToIterate.Add(n);
            }
        }

        finalReachableNodes.UnionWith(nodesToIterate);

        while (nodesToIterate.Count != 0)
        {
            foreach (Node n in nodesToIterate)
            {
                foreach (Node neighbour in n.neighbours)
                {
                    tempCost = PathCostToTile(costType, n.gridPosition, neighbour.gridPosition, gridInfo) + cost[n.gridPosition.x, n.gridPosition.y];
                    currCost = cost[neighbour.gridPosition.x, neighbour.gridPosition.y];
                    //Node doesn't exist in final hash or the cost is less than the current cost
                    if (neighbour != unitInitialNode && (tempCost < currCost || currCost <= 0))
                    {
                        //Set new cost
                        cost[neighbour.gridPosition.x, neighbour.gridPosition.y] = tempCost;
                        //If still has points to move and is not infinity add in the temp HashSet
                        //Note: Converting tempCost to int to disregard the fractional points for diagonals
                        if (tempCost != Mathf.Infinity && availablePoints - (int)tempCost >= 0)
                        {
                            tempReachableNodes.Add(neighbour);
                        }
                    }
                }
            }

            finalReachableNodes.UnionWith(tempReachableNodes);
            nodesToIterate = tempReachableNodes;
            tempReachableNodes = new HashSet<Node>();

        }
        return finalReachableNodes;
    }

    public static HashSet<Node> GetPossibleMovementTiles(GridPosition source, int movementPoints, GridServerInfo gridInfo)
    {
        return GetPossibleTilesBasedInCost(PathCostType.Movement, source, movementPoints, gridInfo);
    }

    public static HashSet<Node> GetPossibleAttackTiles(GridPosition source, int attackRange, GridServerInfo gridInfo)
    {
        return GetPossibleTilesBasedInCost(PathCostType.Attack, source, attackRange, gridInfo);
    }

    public static List<Node> GeneratePathTo(PathCostType costType, GridPosition target, GridPosition source, GridServerInfo gridInfo)
    {
        List<Node> result = null;

        switch (gridInfo.pathFindingAlgorithm)
        {
            case PathFindingAlgorithm.Dijkstra:
                result = GenerateDijkstraPath(costType, target, source, gridInfo);
                break;

        }
        return result;
    }

    private static List<Node> GenerateDijkstraPath(PathCostType costType, GridPosition targetPosition, GridPosition sourcePosition, GridServerInfo gridInfo)
    {
        //Nodes along the path of shortest path from the pathfinding
        List<Node> currentPath;
        if (targetPosition.x == sourcePosition.x && targetPosition.y == sourcePosition.y)
        {
            //Debug.Log("clicked the same tile that the unit is standing on");
            return null;
        }
        if (UnitCanMoveIntoTile(targetPosition, gridInfo.currentMap) == false)
        {
            //cant move into something so we can probably just return
            //cant set this endpoint as our goal

            return null;
        }

        currentPath = null;
        //from wiki dijkstra's
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        Node source = gridInfo.graphMap[sourcePosition.x, sourcePosition.y];
        Node target = gridInfo.graphMap[targetPosition.x, targetPosition.y];
        dist[source] = 0;
        prev[source] = null;
        //Unchecked nodes
        List<Node> unvisited = new List<Node>();

        //Initialize
        foreach (Node n in gridInfo.graphMap)
        {
            //Initialize to infite distance as we don't know the answer
            //Also some places are infinity
            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            unvisited.Add(n);
        }
        //if there is a node in the unvisited list lets check it
        while (unvisited.Count > 0)
        {
            //u will be the unvisited node with the shortest distance
            Node u = null;
            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.neighbours)
            {
                //float alt = dist[u] + u.DistanceTo(n);
                float alt = dist[u] + PathCostToTile(costType, u.gridPosition, n.gridPosition, gridInfo);
                if (alt < dist[n])
                {
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        //if we are here we found shortest path, or no path exists
        if (prev[target] == null)
        {
            //No route;
            return null;
        }
        currentPath = new List<Node>();
        Node curr = target;
        //Step through the current path and add it to the chain
        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }
        //Now currPath is from target to our source, we need to reverse it from source to target.
        currentPath.Reverse();

        return currentPath;
    }

    private static float PathCostToTile(PathCostType costType, GridPosition source, GridPosition target, GridServerInfo gridInfo)
    {
        float cost = Mathf.Infinity;

        switch (costType)
        {
            case PathCostType.Movement:
                cost = PathMovementCostToTile(source, target, gridInfo);
                break;
            case PathCostType.Attack:
                cost = PathAttackCostToTile(source, target, gridInfo);
                break;

            default:
                break;
        }

        return cost;
    }

    private static float PathMovementCostToTile(GridPosition source, GridPosition target, GridServerInfo gridInfo)
    {
        GridTile tileInstance = GridUtils.GetGridTileAtPosition(target, gridInfo.currentMap);

        if (tileInstance != null && GridUtils.CheckIfTileExistInServerMap(target, gridInfo.currentMap) && UnitCanMoveIntoTile(target, gridInfo.currentMap))
        {
            float cost = tileInstance.currentMovementCost;
            if (source.x != target.x && source.y != target.y)
            {
                //Moving diagonally
                cost = cost * gridInfo.diagonalMultiplier;
            }
            return cost;
        }
        return Mathf.Infinity;
    }

    private static float PathAttackCostToTile(GridPosition source, GridPosition target, GridServerInfo gridInfo)
    {
        Node nodeInstance = gridInfo.graphMap[source.x, source.y];
        if (nodeInstance != null && GridUtils.CheckIfTileExistInServerMap(target, gridInfo.currentMap) && UnitCanAttackTile(target, gridInfo.currentMap))
        {

            float cost = nodeInstance.DistanceTo(target);

            if (source.x != target.x && source.y != target.y)
            {
                //Moving diagonally
                cost = cost * gridInfo.diagonalMultiplier;
            }
            return cost;
        }
        return Mathf.Infinity;
    }

    private static bool UnitCanMoveIntoTile(GridPosition tilePosition, GridTile[,] serverMap)
    {
        GridTile tileInstance = GridUtils.GetGridTileAtPosition(tilePosition, serverMap);
        if (tileInstance != null && GridUtils.CheckIfTileExistInServerMap(tilePosition, serverMap))
        {
            return tileInstance.currentIsWalkable && tileInstance.unitInTile == null;
        }
        return false;
    }

    private static bool UnitCanAttackTile(GridPosition tilePosition, GridTile[,] serverMap)
    {
        GridTile tileInstance = GridUtils.GetGridTileAtPosition(tilePosition, serverMap);
        if (tileInstance != null && GridUtils.CheckIfTileExistInServerMap(tilePosition, serverMap))
        {
            return tileInstance.currentIsWalkable;
        }
        return false;
    }


}
