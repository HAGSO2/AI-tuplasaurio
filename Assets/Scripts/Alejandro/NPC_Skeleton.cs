using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC_Skeleton : NPC
{
    private Animator _animator;
    private bool isContact = false;
    
    new void Start()
    {
        base.Start();
        _animator = GetComponent<Animator>();
        _animator.SetBool("isAttacking", false);
    }

    protected override bool IsContact(Vector3 looking) // Returns true if the NPC can hear the player (a sphere collider)
    {
        return isContact;
    }
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.CompareTag("PlayerNoise"))
        {
            //pathfinding.target = other.GameObject();
            _animator.SetBool("isAttacking", true);
            //_manager.ComunicatePlayerLocation(other.transform.position);
            isContact = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerNoise"))
        {
            _animator.SetBool("isAttacking", false);
            isContact = false;
        }
    }

    new void FixedUpdate(){
        base.FixedUpdate();
    }

}
