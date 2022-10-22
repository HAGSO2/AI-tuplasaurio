using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    public bool EnableAlternativePath;
    public Transform targetPos;
    public List<Node> finalPath;

    private void Awake(){
        grid = (FindObjectsOfType<Grid>())[0];
    }

    private void Update(){
        FindPath(this.transform.position,targetPos.position);
    }

    public Vector3[] FindPath(Vector3 startPosition, Vector3 targetPosition){   
        List<Node> openList = new List<Node>();
        List<Node> closeList = new List<Node>();

        grid.ResetGrid();

        grid.NodeFromWorldPosition(startPosition).gCost = 0;
        grid.NodeFromWorldPosition(startPosition).hCost = GetHeuristic(grid.NodeFromWorldPosition(startPosition),  grid.NodeFromWorldPosition(targetPosition));
        openList.Add(grid.NodeFromWorldPosition(startPosition));

        while(openList.Count > 0){
            //Searchs the node with the lowest FCost(hcost + gcost)
            Node currentNode = openList[0];
            for (int i = 1 ; i<openList.Count; i++){
                 if(currentNode.FCost > openList[i].FCost){
                    currentNode = openList[i];
                }
            }

            //If found it
            if(currentNode == grid.NodeFromWorldPosition(targetPosition)){
                return GetFinalPath(grid.NodeFromWorldPosition(startPosition), currentNode).ToArray();
            }

            openList.Remove(currentNode);
            closeList.Add(currentNode);

            //Search neighbors on eight directions
            foreach (Node child in grid.GetNeighborNodes(currentNode)){
                float tentative_gScore = currentNode.gCost + GetHeuristic(currentNode, child);
                if(tentative_gScore < child.gCost && (child.isPath || (EnableAlternativePath && child.isAlternativePath))){
                    child.parent = currentNode;
                    child.gCost = tentative_gScore;
                    child.hCost = GetHeuristic(child, grid.NodeFromWorldPosition(targetPosition));
                    if(!openList.Contains(child)){
                        openList.Add(child);
                    }
                }
            }
        }

        return Array.Empty<Vector3>();
    }

    List<Vector3> GetFinalPath(Node nodeStart, Node nodeEnd){
        List<Node> FinalPath = new List<Node>();
        List<Vector3> FinalPathPositions = new List<Vector3>();
        Node currentNode = nodeEnd;

        while(currentNode != nodeStart){
            FinalPath.Add(currentNode);
            FinalPathPositions.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        FinalPath.Reverse();
        FinalPathPositions.Reverse();

        finalPath = FinalPath;
        grid.paths.Insert(0,FinalPath);
        
        return FinalPathPositions;
    }

    float GetHeuristic(Node nodeA, Node nodeB){
        float ix = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        float iy = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        float d = 1;
        return (d * (ix + iy) + (nodeA.secondDiagonal - 2 * d) * Mathf.Min(ix, iy));
    }
}
