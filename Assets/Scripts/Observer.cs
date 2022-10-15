using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Observer : MonoBehaviour
{
    public Transform player;
    public UnityEvent onSeePlayer;
    //public GameEnding gameEnding;

    bool m_IsPlayerInRange;
    private bool seePlayer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == player)
        {
            m_IsPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == player)
        {
            m_IsPlayerInRange = false;
            seePlayer = false;
        }
    }

    private void Update()
    {
        if (m_IsPlayerInRange && !seePlayer)
        {
            Vector3 direction = player.position - transform.position + Vector3.up;
            Ray ray = new Ray(transform.position, direction);

            RaycastHit raycastHit;

            if (Physics.Raycast(ray, out raycastHit))
            {
                if (raycastHit.collider.transform == player)
                {
                    onSeePlayer.Invoke();
                    seePlayer = true;
                    //gameEnding.CaughtPlayer();
                }
            }
        }
    }

    
    
    
}
