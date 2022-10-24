using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Ghost : NPC
{

    [SerializeField] private float maxDistanceView = 3f;
    [SerializeField] private float angleView = 15f;

    protected override bool IsContact(Vector3 looking) //Returns true if the NPC can see the poitn at a max distance
    {
        Vector3 dir = looking - transform.position;
        RaycastHit raycastHit;
        if (Vector3.Angle(transform.forward, dir) < angleView)
        {
            if (Physics.Raycast(transform.position, dir, out raycastHit, maxDistanceView) && raycastHit.transform.CompareTag("Player"))
            {
                //Debug.Log("He visto al jugador");
                //_manager.ComunicatePlayerLocation(raycastHit.transform.position);
                base._lastPos = looking;
                return true;
            }
        }
        return false;
    }

    //private void PlayerVisible()
}
