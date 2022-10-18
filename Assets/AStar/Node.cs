using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int gridX;
    public int gridY;

    public bool isPath;
    public bool isAlternativePath;
    public Vector3 position;

    public Node parent;
    public int gCost = 99999999; // cost to move to the next square
    public int hCost; // distance to the goal from this node

    public int FCost {get{return gCost + hCost;}}

    public Node (bool IsPath, bool IsAlternativePath, Vector3 Pos, int GridX, int GridY){
        isPath = IsPath;
        isAlternativePath = IsAlternativePath;
        position = Pos;
        gridX = GridX;
        gridY = GridY;
    }

}
