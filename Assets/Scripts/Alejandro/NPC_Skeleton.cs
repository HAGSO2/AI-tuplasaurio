using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC_Skeleton : NPC
{
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.CompareTag("PlayerNoise"))
        {
            //aStar.goTo(other.transform.position)
            //base.
        }
    }
    
}
