using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    private NPC[] _enemies;
    void Start()
    {
        _enemies = gameObject.transform.GetComponentsInChildren<NPC>();
        
    }

    public void ComunicatePlayerLocation(Vector3 position)
    {
        foreach (NPC enemy in _enemies)
        {
            enemy.goToComunicatedLocation(position);
        }
    }

}
