using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatPoint : MonoBehaviour
{
    static public float heatLoss = 0.01f;
    public HeatPoint[] Conections;
    public float heat;
    public bool[] KnownNPC;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 offset = Vector3.up;
        foreach (HeatPoint p in Conections)
        {
            Debug.DrawLine(transform.position + offset, p.transform.position + offset,
                Color.Lerp(Color.blue, Color.red, heat - p.heat));
            offset += Vector3.up;
        }
        //heat -= heat > 0 ? Time.deltaTime * heatLoss : 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            heat = 1.5f;
        }

        /*if (other.transform.CompareTag("NPC"))
        {
            NPC info = other.GetComponent<NPC>();
            if (!KnownNPC[info.id])
            {
                KnownNPC[info.id] = true;
                info.EnqueuePos(this);
            }
        }*/
    }
/*
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("NPC"))
        {
            KnownNPC[other.GetComponent<NPC>().id] = false;
        }
    }
*/
}
