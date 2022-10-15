using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    public Transform targetPos;
    public List<Node> finalPath;

    private void Awake(){
        grid = (FindObjectsOfType<Grid>())[0];
    }

    private void Update(){
        FindPath(this.transform.position,targetPos.position);
    }

    void FindPath(Vector3 startPosition, Vector3 targetPosition){   
        List<Node> openList = new List<Node>();
        List<Node> closeList = new List<Node>();

        grid.ResetGrid();

        grid.NodeFromWorldPosition(startPosition).gCost = 0;
        grid.NodeFromWorldPosition(startPosition).hCost = GetEucledianDistance(grid.NodeFromWorldPosition(startPosition),  grid.NodeFromWorldPosition(targetPosition));
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
                GetFinalPath(grid.NodeFromWorldPosition(startPosition), currentNode);
                break;
            }

            openList.Remove(currentNode);
            closeList.Add(currentNode);

            //Search neighbors on eight directions
            foreach (Node child in grid.GetNeighborNodes(currentNode)){
                int tentative_gScore = currentNode.gCost + GetEucledianDistance(currentNode, child);
                if(tentative_gScore < child.gCost && child.isWall){
                    child.parent = currentNode;
                    child.gCost = tentative_gScore;
                    child.hCost = GetEucledianDistance(child, grid.NodeFromWorldPosition(targetPosition));
                    if(!openList.Contains(child)){
                        openList.Add(child);
                    }
                }
            }
        }        
    }

    void GetFinalPath(Node nodeStart, Node nodeEnd){
        List<Node> FinalPath = new List<Node>();
        Node currentNode = nodeEnd;

        while(currentNode != nodeStart){
            FinalPath.Add(currentNode);
            currentNode = currentNode.parent;
        }

        FinalPath.Reverse();

        finalPath = FinalPath;
        grid.paths.Insert(0,FinalPath);
    }

    int GetEucledianDistance(Node nodeA, Node nodeB){
        int ix = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int iy = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        return (int)(Mathf.Sqrt(Mathf.Pow(ix,2) + Mathf.Pow(iy,2)));
    }
}
