using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HeatPoint : MonoBehaviour
{
    public HeatPoint[] Conections;
    private HeatDir[] _dirs;

    public Vector3 RNext(HeatDir previous)
    {
        int i = Random.Range(0, Conections.Length);
        while (_dirs[i] != previous)
        {
            i = Random.Range(0, Conections.Length);
        }

        return Conections[i].transform.position;
    }
}
