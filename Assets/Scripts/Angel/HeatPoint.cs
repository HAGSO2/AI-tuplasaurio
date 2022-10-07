using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HeatPoint : MonoBehaviour
{
    [SerializeField] private Transform[] adyacent;
    [SerializeField] private HeatDir.Dir[] conection_type;
    private HeatDir[] dirs;

    private void Start()
    {
        dirs = new HeatDir[adyacent.Length];
        for (int i = 0; i < adyacent.Length; i++)
        {
            dirs[i] = new HeatDir(conection_type[i], adyacent[i]);
        }
    }

    public HeatDir RNext(HeatDir previous)
    {
        int i = Random.Range(0, dirs.Length);
        while (dirs[i] != previous)
        {
            i = Random.Range(0, dirs.Length);
        }

        return dirs[i];
    }
}
