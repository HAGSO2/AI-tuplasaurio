using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gargoyle : MonoBehaviour
{
    private BoxCollider _vision;
    private Animator _animator;
    private void Start()
    {
        _vision = GetComponent<BoxCollider>();
        _animator = GetComponent<Animator>();
        StartCoroutine(SleepX_Seconds(5));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Avisar NPC's
            Debug.Log("Seen");
        }
    }

    
    private IEnumerator SleepX_Seconds(float seconds)
    {
        _vision.enabled = !_vision.enabled;
        _animator.enabled = !_animator.enabled;
        yield return new WaitForSeconds(seconds);

        yield return StartCoroutine(SleepX_Seconds(5));
        
    }
}
