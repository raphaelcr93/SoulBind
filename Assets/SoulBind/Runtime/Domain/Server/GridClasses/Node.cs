using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node { 
    
        public List<Node> neighbours;
        public GridPosition gridPosition;
        //public int x;
        //public int y;
        //Edges
        public Node(int x, int y)
        {
            neighbours = new List<Node>();
            gridPosition = new GridPosition(x,y);
        }

        public float DistanceTo(Node n)
        {
            return DistanceTo(n.gridPosition);
        }

        public float DistanceTo(GridPosition targetGridPosition)
        {
            return Vector2.Distance(new Vector2(gridPosition.x, gridPosition.y), new Vector2(targetGridPosition.x, targetGridPosition.y));
        }


    
}
