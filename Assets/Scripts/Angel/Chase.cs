using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class Chase : MonoBehaviour
{
    [SerializeField] private Transform reference;
    //[SerializeField] private Transform seek;
    private List<Transform> _objects;
    [SerializeField]private Transform _player;
    private Rigidbody _body;
    [SerializeField]private float _vel;
    private Pathfinding _pathfinding;
    private bool _followinP;
    //private Vector3 _movDir;
    private Vector3 _lastPos;
    private void Start()
    {
        _body = GetComponent<Rigidbody>();
        _pathfinding = GetComponent<Pathfinding>();
        _objects = new List<Transform>();
        _lastPos = Vector3.up;
    }
    // Start is called before the first frame update
    private void set_speed(Vector3 runnigAt)//if stop == true will stop on the point is running, if not, will not stop
    {
        //Changes speed direction to go to the runningPoint
        runnigAt.y = transform.position.y;
        Vector3 position = transform.position;
        Vector3 desired = runnigAt - position;
        Vector3 steering = desired - _body.velocity;
        Debug.DrawRay(position,desired,Color.blue);
        Debug.DrawRay(position,steering.normalized * _vel,Color.red);
        if (Vector3.Angle(desired, steering) < 30 && Vector3.Distance(runnigAt,position) > 1)
        {
            _body.AddForce(steering.normalized * _vel);
            //_movDir = steering.normalized;
        }
        else
            _body.AddForce(desired.normalized * _vel);

    }
    private bool is_Contact(Vector3 looking) //Returns true if the NPC can see the poitn at a max distance
    {
        Vector3 dir = looking - transform.position;
        RaycastHit raycastHit;
        if (Physics.Raycast(transform.position, dir, out raycastHit) && raycastHit.transform.CompareTag("Player"))
        {
            _lastPos = raycastHit.point;
            return true;
        }

        return false;
    }
    private IEnumerator FollowPath(Vector3[] path)
    {
        _followinP = true;
        foreach (Vector3 coord in path)
        {

            while (Vector3.Distance(coord, transform.position) > 1.8f)
            {
                yield return new WaitUntil(() => _body.velocity.magnitude < 5);
                is_Contact(_player.position);
                set_speed(coord);
                yield return new WaitForFixedUpdate();
            }
        }

        _followinP = false;
    }

    private void CreateRefs(Vector3[] path)
    {
        foreach (Vector3 coord in path)
        {
            _objects.Add(Instantiate(reference));
            _objects[^1].position = coord;

        }
    }

    private void FixedUpdate()
    {
        if (!_followinP && is_Contact(_player.position))
        {
            Vector3[] path = _pathfinding.FindPath(transform.position, _player.position);
            StartCoroutine(FollowPath(path));
            CreateRefs(path);
            //FollowPath(_pathfinding.FindPath(transform.position,_target.position));
        }
        else if(!_followinP && _lastPos != Vector3.up)
        {
            Debug.Log("Search on: " + _lastPos);
            Vector3[] path = _pathfinding.FindPath(transform.position, _lastPos);
            StartCoroutine(FollowPath(path));
            CreateRefs(path);
            _lastPos = Vector3.up;
        }
    }
}
