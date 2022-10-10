using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC_Skeleton : NPC
{
    private Animator _animator;
    public static bool playerSeen = false;
    public static Vector3 playerPosition;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.SetBool("isAttacking", false);
    }


    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.CompareTag("PlayerNoise"))
        {
            
            //pathfinding.target = other.GameObject();
            playerSeen = true;
            _animator.SetBool("isAttacking", true);
            playerPosition = other.transform.position;
        }
    }

    private void Update()
    {
        if (playerSeen)
        {
            //FollowPath(pathfinding.finalPath);
        }
    }

    private void FollowPath(List<Vector3> path)
    {
        foreach (Vector3 coord in path)
        {
            while (DistanceLessThan(0.85f, coord))
            {
                MoveTo(coord);
            }
        }
    }
    
}
