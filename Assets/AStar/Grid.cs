using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask WallMask;
    public LayerMask AtravesableWallMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public float distance;

    Node[,]grid;
    public List<List<Node>> paths = new List<List<Node>>();

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    private void Start(){
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
        CreateGrid();
    }

    private void Update(){
        CreateGrid();
    }

    void CreateGrid(){
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++){
            for (int y = 0; y < gridSizeY; y++){
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool isPath = true;
                bool isAlternativePath = false;
                if(Physics.CheckSphere(worldPoint, nodeRadius, WallMask)){
                    isPath = false;
                }else if(Physics.CheckSphere(worldPoint, nodeRadius, AtravesableWallMask)){
                    isPath = false;
                    isAlternativePath = true;
                }

                grid[x,y] = new Node(isPath,isAlternativePath,worldPoint,x,y);
            }   
        }
    }

    private void OnDrawGizmos(){
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x,1,gridWorldSize.y));
        if(grid != null){
            foreach(Node node in grid){
                if(node.isPath){
                    Gizmos.color = Color.green;
                }else if(node.isAlternativePath){
                    Gizmos.color = Color.yellow;
                }
                else{
                    Gizmos.color = Color.red;
                }

                foreach(List<Node> path in paths){
                    if(path.Contains(node)){
                        Gizmos.color = Color.blue;
                    }
                }

                Gizmos.DrawCube(node.position, Vector3.one * (nodeDiameter - distance));
            }
        }
        paths.Clear();
    }

    public Node NodeFromWorldPosition(Vector3 WorldPos){
        float xPoint = ((WorldPos.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float yPoint = ((WorldPos.z + gridWorldSize.y / 2) / gridWorldSize.y);

        xPoint = Mathf.Clamp01(xPoint);
        yPoint = Mathf.Clamp01(yPoint);

        int x = Mathf.RoundToInt((gridSizeX - 1) * xPoint);
        int y = Mathf.RoundToInt((gridSizeY - 1) * yPoint);

        return grid[x,y];
    }

    public List<Node> GetNeighborNodes(Node n){
        List<Node> neighborList = new List<Node>();
        int xCheck;
        int yCheck;

        xCheck  = n.gridX + 1;
        yCheck = n.gridY;
        if(xCheck >= 0 && xCheck < gridSizeX && yCheck >= 0 && yCheck < gridSizeY){
            neighborList.Add(grid[xCheck,yCheck]);
        }


        xCheck  = n.gridX - 1;
        yCheck = n.gridY;
        if(xCheck >= 0 && xCheck < gridSizeX && yCheck >= 0 && yCheck < gridSizeY){
            neighborList.Add(grid[xCheck,yCheck]);
        }


        xCheck  = n.gridX;
        yCheck = n.gridY + 1;
        if(xCheck >= 0 && xCheck < gridSizeX && yCheck >= 0 && yCheck < gridSizeY){
            neighborList.Add(grid[xCheck,yCheck]);
        }


        xCheck  = n.gridX;
        yCheck = n.gridY - 1;
        if(xCheck >= 0 && xCheck < gridSizeX && yCheck >= 0 && yCheck < gridSizeY){
            neighborList.Add(grid[xCheck,yCheck]);
        }


        xCheck  = n.gridX-1;
        yCheck = n.gridY - 1;
        if(xCheck >= 0 && xCheck < gridSizeX && yCheck >= 0 && yCheck < gridSizeY){
            neighborList.Add(grid[xCheck,yCheck]);
        }


        xCheck  = n.gridX+1;
        yCheck = n.gridY - 1;
        if(xCheck >= 0 && xCheck < gridSizeX && yCheck >= 0 && yCheck < gridSizeY){
            neighborList.Add(grid[xCheck,yCheck]);
        }


        xCheck  = n.gridX+1;
        yCheck = n.gridY + 1;
        if(xCheck >= 0 && xCheck < gridSizeX && yCheck >= 0 && yCheck < gridSizeY){
            neighborList.Add(grid[xCheck,yCheck]);
        }


        xCheck  = n.gridX -1;
        yCheck = n.gridY + 1;
        if(xCheck >= 0 && xCheck < gridSizeX && yCheck >= 0 && yCheck < gridSizeY){
            neighborList.Add(grid[xCheck,yCheck]);
        }

        return neighborList;
    }

    public void ResetGrid(){
        foreach (Node n in grid){
            n.gCost = 999999;
            n.hCost = 0;
            n.parent = null;
        }
    }
}
