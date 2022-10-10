using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Ghost : NPC
{
    public static bool playerSeen = false;
    public static Vector3 playerPosition;
    
    private void Update()
    {
        if (playerSeen)
        {
            //FollowPath(pathfinding.finalPath);
        }
    }
    
    //private void PlayerVisible()
}
