using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class Chase : MonoBehaviour
{
    //[SerializeField] private Transform seek;
    [SerializeField] private Transform reference;
    private List<Transform> _objects;
    [SerializeField]private Transform _player;
    private Rigidbody _body;
    [SerializeField]private float _vel;
    private Pathfinding _pathfinding;
    private bool _followingP;
    private Vector3 _lastPos;
    private float _dist;
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
        else if(_body.velocity.magnitude < 5)
            _body.AddForce(desired.normalized * _vel);

    }
    private void _rotate(Vector3 target)
    {
        if (_body.velocity != Vector3.zero)
        {
            Vector3 goal = new Vector3(target.x, transform.position.y, target.z);

            Vector3 direction = goal - transform.position;
            direction.Normalize();

            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, direction, 5 * Time.fixedDeltaTime, 0f);

            _body.MoveRotation(Quaternion.LookRotation(desiredForward));
        }
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
        _followingP = true;
        foreach (Vector3 coord in path)
        {

            while (Vector3.Distance(coord, transform.position) > 1.8f)
            {
                yield return new WaitForFixedUpdate();
                is_Contact(_player.position);
                set_speed(coord);
                if(_lastPos == Vector3.up)
                    _rotate(coord);
                if (_dist < 2.8f && is_Contact(_player.position))
                {
                    _followingP = false;
                    break;
                }
            }
        }

        _followingP = false;
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
        _dist = Vector3.Distance(transform.position, _player.position);
        Debug.Log(_dist);
        if(_lastPos != Vector3.up)
            _rotate(_player.position);
        if (_dist < 2.8f && is_Contact(_player.position))
        {
            set_speed(_player.position);
            if (_dist < 1.5f)
            {
                Debug.Log("Player chased");
                Destroy(this);
            }
        }

        if (!_followingP && is_Contact(_player.position))
        {
            Vector3[] path = _pathfinding.FindPath(transform.position, _player.position);
            StartCoroutine(FollowPath(path));
            CreateRefs(path);
            //FollowPath(_pathfinding.FindPath(transform.position,_target.position));
        }
        else if(!_followingP && _lastPos != Vector3.up)
        {
            Debug.Log("Search on: " + _lastPos);
            Vector3[] path = _pathfinding.FindPath(transform.position, _lastPos);
            StartCoroutine(FollowPath(path));
            CreateRefs(path);
            _lastPos = Vector3.up;
        }
        else if(!_followingP && _lastPos == Vector3.up)
            Destroy(this);
    }
}
