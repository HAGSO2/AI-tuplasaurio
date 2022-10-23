using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    private NPC[] _enemies;
    private List<EnemyComparer> _comparableEnemies = new();
    void Start()
    {
        _enemies = gameObject.transform.GetComponentsInChildren<NPC>();
        
    }

    public void ComunicatePlayerLocation(Vector3 playerPosition)
    {
        
        foreach (NPC enemy in _enemies)
        {
            enemy._pathfinding.FindPath(enemy.transform.position, playerPosition);
            var distance = enemy._pathfinding.finalPath.Count;
            var e = new EnemyComparer
            {
                Enemy = enemy,
                Distance = distance
            };
            _comparableEnemies.Add(e);
        }

        FindXClosestEnemies(2, playerPosition);
        
        //StartCoroutine(GoToLv2Seek(5, playerPosition));
    }

    private void FindXClosestEnemies(int x, Vector3 playerPosition)
    {
        
        Debug.Log("__________________________________________________________________________________________________________________");
        Debug.Log("Total Enemies: " + _enemies.Length);
        x %= _enemies.Length + 1;
        Debug.Log("Enemies after you: " + x);
        _comparableEnemies.Sort();
        for (int i = 0; i < x; i++)
        {
            
            Debug.Log("Enemy: " + i + " -> " + _comparableEnemies[i].Enemy.name + "\n" + "Distance: " + _comparableEnemies[i].Distance);
            _comparableEnemies[i].Enemy.goToComunicatedLocation(playerPosition);
        }
        
        Debug.Log("__________________________________________________________________________________________________________________");
    }

    private IEnumerator GoToLv2Seek(int seconds, Vector3 playerPosition)
    {
        yield return new WaitForSeconds(seconds);

        Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        FindXClosestEnemies(2, playerPosition);
    }

}

public class EnemyComparer : IComparable<EnemyComparer>
{
    public NPC Enemy { get; set; }
    public int Distance { get; set; }
    private IComparable<EnemyComparer> _comparableImplementation;
    public int CompareTo(EnemyComparer other)
    {
        return this.Distance.CompareTo(other.Distance);
    }
}
