using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class NPC : MonoBehaviour
{
    public Transform player;
    private Rigidbody _body;
    private RaycastHit _toPlayer;
    private List<NPC> _nearNpCs;
    private float _heatCrumbs; // At the time og making a heat map...
    //private float _maxDist; // Will be used in skeletons
    private float _vel = 5.5f; // Velocity max magnitude
    public bool Alarmed { get; private set; }

    private void set_speed(Vector3 runnigAt)
    {
        //Changes speed direction to go to the runningPoint

        Vector3 desired = runnigAt - transform.position;
        Vector3 steering = desired - _body.velocity;
        steering.Normalize();
        _body.AddForce(steering * _vel);
    }

    private void set_Orientation(Vector3 looking, float time)
    {
        Vector3 desired = looking - transform.position;
        transform.forward = Vector3.Lerp(transform.forward, desired.normalized, time);
    }
    // Start is called before the first frame update
    void Start()
    {
        _body = gameObject.GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        set_Orientation(player.position,Time.deltaTime);
        set_speed(player.position);
    }
}
