using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Walk : MonoBehaviour
{
    private Rigidbody _body;
    [SerializeField]private float _vel;
    public Transform Target;

    private void Start()
    {
        _body = GetComponent<Rigidbody>();
        set_speed(Target.position);
    }

    private void set_speed(Vector3 runnigAt)//if stop == true will stop on the point is running, if not, will not stop
    {
        //Changes speed direction to go to the runningPoint

        Vector3 position = transform.position;
        Vector3 desired = runnigAt - position;
        Vector3 steering = desired - _body.velocity;
        Debug.DrawRay(position,steering.normalized * _vel,Color.red);
        steering.Normalize();
        _body.AddForce(steering * _vel);
    }

    private void FixedUpdate()
    {
        set_speed(Target.position);
    }
}
