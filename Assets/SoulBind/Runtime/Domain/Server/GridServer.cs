using System;
using System.Collections.Generic;

public class GridServer
{


    /*************************************************/
    /****Grid Initialization Variables and Methods****/
    /*************************************************/
    #region Grid Initialization Variables and Methods

    GridServerInfo gridInfo;
    GridTileType[] gridTileTypes;

    public GridServer()
    {
        InitializeAttributes();
    }

    void InitializeAttributes()
    {
        gridInfo = new GridServerInfo();
        GenerateTileTypes();
        GenerateOriginalMapInfo();
        GeneratePathFindingGraph();
    }

    void GenerateTileTypes()
    {
        gridTileTypes = new GridTileType[Enum.GetValues(typeof(GridTileType.GridTileTypeName)).Length];
        gridInfo.TilesPrefabsToLoad = new List<string>();
        string tilePrefabPath = "Assets/Prefabs/Grid/GridTile.prefab";
        gridInfo.TilesPrefabsToLoad.Add(tilePrefabPath);

        GridTileType temp = new GridTileType(tilePrefabPath, 1, true, 0.2148452f, 0.6415094f, 0.2166689f);
        gridTileTypes[(int)GridTileType.GridTileTypeName.Normal] = temp;

        temp = new GridTileType(tilePrefabPath, 1, false, 0.5754717f, 0.4316038f, 0.4316038f);
        gridTileTypes[(int)GridTileType.GridTileTypeName.Mountain] = temp;

        temp = new GridTileType(tilePrefabPath, 2, true, 0.2358491f, 0.2358491f, 0.2358491f);
        gridTileTypes[(int)GridTileType.GridTileTypeName.Swamp] = temp;
    }

    void GenerateOriginalMapInfo()
    {
        int gridSizeX = 10;
        int gridSizeY = 10;

        gridInfo.currentMap = new GridTile[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                gridInfo.currentMap[x, y] = new GridTile(gridTileTypes[(int)GridTileType.GridTileTypeName.Normal], new GridPosition(x, y));
                //Start all tiles as Normal
            }
        }

        //Make U shape Mountain
        gridInfo.currentMap[2, 6].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Mountain]);
        gridInfo.currentMap[2, 5].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Mountain]);
        gridInfo.currentMap[2, 4].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Mountain]);
        gridInfo.currentMap[3, 4].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Mountain]);
        gridInfo.currentMap[4, 4].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Mountain]);
        gridInfo.currentMap[5, 4].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Mountain]);
        gridInfo.currentMap[5, 5].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Mountain]);
        gridInfo.currentMap[5, 6].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Mountain]);

        //Make a sqaure swamp
        gridInfo.currentMap[7, 3].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Swamp]);
        gridInfo.currentMap[7, 4].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Swamp]);
        gridInfo.currentMap[7, 5].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Swamp]);
        gridInfo.currentMap[8, 3].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Swamp]);
        gridInfo.currentMap[8, 4].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Swamp]);
        gridInfo.currentMap[8, 5].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Swamp]);
        gridInfo.currentMap[9, 3].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Swamp]);
        gridInfo.currentMap[9, 4].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Swamp]);
        gridInfo.currentMap[9, 5].SetTileType(gridTileTypes[(int)GridTileType.GridTileTypeName.Swamp]);
    }

    void GeneratePathFindingGraph()
    {
        int gridSizeX = gridInfo.currentMap.GetLength(0);
        int gridSizeY = gridInfo.currentMap.GetLength(1);

        gridInfo.graphMap = new Node[gridSizeX, gridSizeY];

        //initialize graph 
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (GridUtils.CheckIfTileExistInServerMap(x, y, gridInfo.currentMap))
                {
                    gridInfo.graphMap[x, y] = new Node(x, y);
                }
            }
        }
        //calculate neighbours
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                //X is not 0, then we can add left (x - 1)
                if (x > 0 && GridUtils.CheckIfTileExistInServerMap(x - 1, y, gridInfo.currentMap))
                {
                    gridInfo.graphMap[x, y].neighbours.Add(gridInfo.graphMap[x - 1, y]);
                    if (gridInfo.enableDiagonals)
                    {
                        if (y > 0 && GridUtils.CheckIfTileExistInServerMap(x - 1, y - 1, gridInfo.currentMap))
                            gridInfo.graphMap[x, y].neighbours.Add(gridInfo.graphMap[x - 1, y - 1]);
                        if (y < gridSizeY - 1 && GridUtils.CheckIfTileExistInServerMap(x - 1, y + 1, gridInfo.currentMap))
                            gridInfo.graphMap[x, y].neighbours.Add(gridInfo.graphMap[x - 1, y + 1]);
                    }
                }
                //X is not mapSizeX - 1, then we can add right (x + 1)
                if (x < gridSizeX - 1 && GridUtils.CheckIfTileExistInServerMap(x + 1, y, gridInfo.currentMap))
                {
                    gridInfo.graphMap[x, y].neighbours.Add(gridInfo.graphMap[x + 1, y]);
                    if (gridInfo.enableDiagonals)
                    {
                        if (y > 0 && GridUtils.CheckIfTileExistInServerMap(x + 1, y - 1, gridInfo.currentMap))
                            gridInfo.graphMap[x, y].neighbours.Add(gridInfo.graphMap[x + 1, y - 1]);
                        if (y < gridSizeY - 1 && GridUtils.CheckIfTileExistInServerMap(x + 1, y + 1, gridInfo.currentMap))
                            gridInfo.graphMap[x, y].neighbours.Add(gridInfo.graphMap[x + 1, y + 1]);
                    }
                }
                //Y is not 0, then we can add downwards (y - 1 ) 
                if (y > 0 && GridUtils.CheckIfTileExistInServerMap(x, y - 1, gridInfo.currentMap))
                {
                    gridInfo.graphMap[x, y].neighbours.Add(gridInfo.graphMap[x, y - 1]);
                }
                //Y is not mapSizeY -1, then we can add upwards (y + 1)
                if (y < gridSizeY - 1 && GridUtils.CheckIfTileExistInServerMap(x, y + 1, gridInfo.currentMap))
                {
                    gridInfo.graphMap[x, y].neighbours.Add(gridInfo.graphMap[x, y + 1]);
                }
            }
        }
    }

    public GridServerInfo GetGridInfo()
    {
        return gridInfo;
    }

    #endregion

}

public class GridServerInfo
{
    /***************************/
    /****Grid Configurations****/
    /***************************/
    #region  Grid Configurations

    //If true the grid will allow movement in diagonals
    public bool enableDiagonals = false;
    //This is the number the path algorithm will multiply the cost to move in the terrain in case it is a diagonal.
    public float diagonalMultiplier = 2.0001f;
    //Algorithm to use for Pathfinding Operations
    public PathFindingUtils.PathFindingAlgorithm pathFindingAlgorithm = PathFindingUtils.PathFindingAlgorithm.Dijkstra;

    //This variable is just used to initialize the map with the right types
    public GridTile[,] currentMap;
    //Node graph that will hold the neighbours for each position
    public Node[,] graphMap;
    //List of prefabs that needs to be loaded
    public List<string> TilesPrefabsToLoad;

    #endregion
}