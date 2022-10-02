using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class NPC : MonoBehaviour
{
    public int id = 0;
    public Transform player;
    private Rigidbody _body;
    private RaycastHit _toPlayer;
    private List<NPC> _nearNpCs;
    private float _heatCrumbs; // At the time og making a heat map...
    public float maxDist; // Will be used in skeletons
    private float _vel = 1.5f; // Velocity max magnitude
    private Vector3 _movDir;
    private Queue<Vector3> PendingPositions;
    public bool Alarmed { get; private set; }

    private void set_speed(Vector3 runnigAt)
    {
        //Changes speed direction to go to the runningPoint

        Vector3 desired = runnigAt - transform.position;
        Vector3 steering = desired - _body.velocity;
        Debug.DrawLine(transform.position,player.position,Color.black);
        Debug.DrawRay(transform.position,steering.normalized * _vel);
        steering.Normalize();
        if(Vector3.Angle(desired,steering) < 30 || (transform.position - runnigAt).magnitude > 10)
        {
            _movDir = steering;
            _body.AddForce(steering * _vel);
        }
        else
        {
            _body.AddForce(_movDir * _vel);
        }
    }

    private void set_Orientation(Vector3 looking, float time)// Turns the NPC to the point
    {
        Vector3 desired = looking - transform.position;
        transform.forward = Vector3.Lerp(transform.forward, desired.normalized, time);
    }

    private bool is_Contact(Vector3 looking) //Returns true if the NPC can see the poitn at a max distance
    {
        return Physics.Raycast(transform.position, (looking - transform.position),maxDist);
    }
    // Start is called before the first frame update
    void Start()
    {
        _body = gameObject.GetComponent<Rigidbody>();
        PendingPositions = new Queue<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        /*set_Orientation(player.position,Time.deltaTime);
        set_speed(player.position);*/
    }
    public void EnqueuePos(HeatPoint Crumb)
    {
        PendingPositions.Enqueue(Crumb.transform.position);
        StartCoroutine(GetTo(Crumb.transform.position));
        StartCoroutine(SearchHeat(Crumb,0));
    }

    private IEnumerator GetTo(Vector3 position)
    {
        set_speed(position);
        yield return new WaitUntil(() => _body.velocity.magnitude < 5);
        if ((transform.position - position).magnitude > 0.2f)
            StartCoroutine(GetTo(position));
        else if (PendingPositions.Count > 0)
        {
            PendingPositions.Dequeue();
            StartCoroutine(GetTo(PendingPositions.Peek()));
        }
    }

    private IEnumerator SearchHeat(HeatPoint Crumb, float time)
    {
        Debug.Log(Crumb.gameObject.name);
        if(Crumb.Conections.Length == 0)
            yield break;
        if (time > 5)
        {
            time = 0;
            yield return new WaitForEndOfFrame();
        }
        float maxHeat = 0;
        int hottest = 0;
        for (int i = 1; i < Crumb.Conections.Length; i++)
        {
            if (Crumb.Conections[i].heat > maxHeat)
                hottest = i;
        }

        Debug.Log("Hottest: " + Crumb.Conections[hottest].gameObject.name);
        if (maxHeat > 0)
        {
            Crumb.Conections[hottest].KnownNPC[id] = true;
            PendingPositions.Enqueue(Crumb.Conections[hottest].transform.position);
            time += Time.deltaTime;
            StartCoroutine(SearchHeat(Crumb.Conections[hottest],time));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HeatCrumb"))
        {
            PendingPositions.Dequeue();
        }
    }
}
