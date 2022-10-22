using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Owl : MonoBehaviour
{
    [SerializeField] protected EnemiesManager _manager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _manager.ComunicatePlayerLocation(other.transform.position);
        }
    }
}
