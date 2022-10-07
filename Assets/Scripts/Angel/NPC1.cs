using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class NPC1 : MonoBehaviour
{
    [SerializeField]private Transform player;
    private Rigidbody _body;
    private RaycastHit _toPlayer;
    private HeatDir _remember;
    [SerializeField] private float _vel = 1.5f; // Velocity max magnitude
    private Vector3 _movDir;
    void Start()
    {
        _body = gameObject.GetComponent<Rigidbody>();
        _remember = new HeatDir(HeatDir.Dir.Unknow);
        Chase();
    }

    void Chase()
    {
        if (is_Contact(player.position))
        {
            StartCoroutine(GetTo(player, false));
        }
        else
        {
            //Investigate()
        }
    }

    void FixedUpdate()
    {
        if(is_Contact(player.position))
            set_Orientation(player,Time.deltaTime);
    }
    
    private void set_speed(Vector3 runnigAt, bool stop)//if stop == true will stop on the point is running, if not, will not stop
    {
        //Changes speed direction to go to the runningPoint

        Vector3 position = transform.position;
        Vector3 desired = runnigAt - position;
        Vector3 steering = desired - _body.velocity;
        Debug.DrawLine(position,player.position,Color.black);
        Debug.DrawRay(position,steering.normalized * _vel);
        steering.Normalize();
        if (Vector3.Angle(desired, steering) < 30 || (stop && (transform.position - runnigAt).magnitude > 10))
        {
            _movDir = steering;
            _body.AddForce(steering * _vel);
        }
        else
        {
            _body.AddForce(_movDir * _vel);
        }
    }

    private void set_Orientation(Transform looking, float time)// Turns the NPC to the point
    {
        Vector3 desired = looking.position - transform.position;
        transform.forward = Vector3.Lerp(transform.forward, desired.normalized, time);
    }

    private bool is_Contact(Vector3 looking) //Returns true if the NPC can see the poitn at a max distance
    {
        return Physics.Raycast(transform.position, (looking - transform.position));
    }
    
    private IEnumerator GetTo(Transform desired, bool stop)
    {
        set_speed(desired.position,stop);
        yield return new WaitUntil(() => _body.velocity.magnitude < 5);
        if ((transform.position - desired.position).magnitude > 0.2f && is_Contact(player.position))
            StartCoroutine(GetTo(desired,stop));
        else if (_remember.NotNull())
            StartCoroutine(GetTo(_remember.NextPoint, false));
        else
        {
            Chase();
        }
    }
}
