using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    private NPC[] _enemies;
    private List<EnemyComparer> _comparableEnemies = new();
    private int _timesSeen = 0;
    void Start()
    {
        _enemies = gameObject.transform.GetComponentsInChildren<NPC>();
        
    }

    public void ComunicatePlayerLocation(Vector3 playerPosition)
    {
        _comparableEnemies = new List<EnemyComparer>();
        foreach (NPC enemy in _enemies)
        {
            //enemy._pathfinding.FindPath(enemy.transform.position, playerPosition);
            var distance = enemy._pathfinding.finalPath.Count;
            var e = new EnemyComparer
            {
                Enemy = enemy,
                Distance = distance
            };
            _comparableEnemies.Add(e);
        }
        _timesSeen += 1;
        Debug.Log("Times Seen player: " + _timesSeen);
        
        FindXClosestEnemies(_timesSeen/2, playerPosition);
    }

    private void FindXClosestEnemies(int x, Vector3 playerPosition)
    {
        
        Debug.Log("__________________________________________________________________________________________________________________");
        Debug.Log("Total Enemies: " + _enemies.Length);
        if (x > _enemies.Length) x = _enemies.Length;
        Debug.Log("Enemies after you: " + x);
        _comparableEnemies.Sort();
        for (int i = 0; i < x; i++)
        {
            
            Debug.Log("Enemy: " + i + " -> " + _comparableEnemies[i].Enemy.name + "\n" + "Distance: " + _comparableEnemies[i].Distance);
            _comparableEnemies[i].Enemy.goToComunicatedLocation(playerPosition);
        }
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
