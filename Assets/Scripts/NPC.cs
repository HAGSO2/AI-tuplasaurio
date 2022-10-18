using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class NPC : MonoBehaviour
{
    private Pathfinding _pathfinding;
    
    [Header("Patrolling")]
    [SerializeField] private Transform waypointContainer;
    private Transform[] _waypoints;
    private int chosenWaypoint;
    private bool waypointReached = true;
    private Vector3[] toWaypoint;
    private int toWaypointIndex = 0;

    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float turnSpeed;

    private bool movingForward;
    private bool stopped;

    private float stoppedTimer;
    [SerializeField]
    private float stoppedMaxTime;

    private Rigidbody rb;

    
    
    [Header("Investigation")]
    [Tooltip("Choose view range")]
    [SerializeField] private float _investigationMaxDistance = 15;
    [SerializeField] private float _investigationMinDistance = 8;
    [SerializeField] private float _viewRange = 45;

    [Tooltip("Select the layer you want the NPC to use")]
    [SerializeField] LayerMask layerMaskForInvestigation;
    private bool investigationPointChosen = false;
    private Vector3 walkPoint;
    private Vector3[] investigationPoints;
    private int _investigationIndex;


    [FormerlySerializedAs("player")]
    [Header("Chasing")] 
    [SerializeField] private Transform _player;
    [SerializeField]private float _vel;
    private bool _followingP;
    private Vector3 _lastPos;
    private float _dist;

    //private Queue<Vector3> _pendingPositions;
    //private Vector3 lastPos;
    [SerializeField] private GameEnding gameEnding;  
    
    private bool isPatrolling = true;
    private bool isInvestigating = false;
    private bool isChasing = false;

    protected void Start()
    {
        _pathfinding = GetComponent<Pathfinding>();
        _followingP = false;
        stoppedTimer = 0;
        chosenWaypoint = 0;

        stopped = false;
        movingForward = true;
        rb = GetComponent<Rigidbody>();

        _waypoints = new Transform[waypointContainer.childCount];

        for (int i = 0; i < waypointContainer.childCount; i++)
        {
            _waypoints[i] = waypointContainer.GetChild(i);
            _waypoints[i].gameObject.SetActive(false);
        }
        

        _waypoints[chosenWaypoint].gameObject.SetActive(true);
    }

    //Patrol --> Chase
    public void StartChase()
    {
        Debug.Log("Start chase");
        isPatrolling = false;
        isInvestigating = false;
        investigationPointChosen = false;
        waypointReached = true;
        isChasing = true;
    }
    
    //Chase --> investigation
    private void LostPlayer()
    {
        Debug.Log("End Chasing");
        Debug.Log("Investigating");
        isChasing = false;
        isInvestigating = true;
    }
    
    //Investigation --> Patrol
    void EndInvestigation() // Only executed when NPC is bored
    {
        Debug.Log("Finished investigating");
        Debug.Log("Start Patrol");

        waypointReached = false;
        toWaypoint = _pathfinding.FindPath(transform.position, _waypoints[chosenWaypoint].position);

        investigationPointChosen = false;
        isInvestigating = false;
        isPatrolling = true;
    }

    protected void FixedUpdate()
    {
        if(isPatrolling)
            Patrolling();
        else if(isInvestigating)
            Investigating();
        else if(isChasing)
            Chasing();
        //Debug.Log(rb.velocity);
        //Set_speed(new Vector3(20,0,20));
    }


    private void Patrolling()
    {
        if (waypointReached)
        {
            if (!stopped)
            {
                MoveTo(_waypoints[chosenWaypoint].position, 1f);
            }
            else
            {
                stoppedTimer += Time.deltaTime;
                if (stoppedTimer >= stoppedMaxTime)
                    stopped = false;
            }
        }
        else
        {
            if (toWaypointIndex < toWaypoint.Length)
            {
                MoveToWithPathFinding(ref toWaypoint, ref toWaypointIndex);
            }
            else
            {
                waypointReached = true;
            }
        }
        Seek_For_Player();

    }

    void Seek_For_Player()
    {
        RaycastHit raycastHit;
        if (Physics.Raycast(transform.position, _player.position - transform.position, out raycastHit) &&
            raycastHit.transform.CompareTag("Player"))
        {
            //Debug.Log(Vector3.Angle(transform.forward,raycastHit.point - transform.position));
            if (Vector3.Angle(transform.forward, raycastHit.point - transform.position) < 60)
            {
                StartChase();
                Debug.DrawLine(transform.position, raycastHit.point, Color.green);
            }
            else
                Debug.DrawLine(transform.position,raycastHit.point,Color.yellow);
                
        }
        else
            Debug.DrawLine(transform.position,raycastHit.point,Color.red);
    }
    void Investigating()
    {
        if (!investigationPointChosen)    // WHEN INVESTIGATING CHOSE A DIRECTION
        {
            walkPoint = GetInvestigationPoint();
            Debug.Log(walkPoint);
   
            // Check if this random direction is valid
            if (Physics.Raycast(walkPoint + Vector3.up, -transform.up, 20f, layerMaskForInvestigation))
            {
                investigationPointChosen = true;
                investigationPoints = _pathfinding.FindPath(transform.position, walkPoint);
            }
            else
            {
                Debug.Log("HELP");
                Debug.Log(walkPoint);
            }
        }
        else
        {
            if (_investigationIndex < investigationPoints.Length)
            {
                MoveToWithPathFinding(ref investigationPoints, ref _investigationIndex);
            }
            else
            {
                EndInvestigation();
            }
        }
    }
    void Chasing()
    {
        _dist = Vector3.Distance(transform.position, _player.position);
        //Debug.Log(_dist);
        if(_lastPos != Vector3.up)
            Rotate(_player.position);
        if (_dist < 1.8f && is_Contact(_player.position))
        {
            MoveTo(_player.position,1);
            if (_dist < 0.3f)
            {
                Debug.Log("Player chased");
                gameEnding.CaughtPlayer();
            }
        }
        else if (!_followingP)
        {
            Vector3[] path = _pathfinding.FindPath(transform.position, _player.position);
            if(is_Contact(_player.position))
                StartCoroutine(FollowPath(path));
            else if (_lastPos != Vector3.up)
            {
                StartCoroutine(FollowPath(path));
                _lastPos = Vector3.up;
            }
            else if(!_followingP && _lastPos == Vector3.up)
                LostPlayer();
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
    private void set_speed(Vector3 runnigAt)//if stop == true will stop on the point is running, if not, will not stop
    {
        //Changes speed direction to go to the runningPoint
        runnigAt.y = transform.position.y;
        Vector3 position = transform.position;
        Vector3 desired = runnigAt - position;
        Vector3 steering = desired - rb.velocity;
        Debug.DrawRay(position,desired,Color.blue);
        Debug.DrawRay(position,steering.normalized * _vel,Color.red);
        if (Vector3.Angle(desired, steering) < 30 && Vector3.Distance(runnigAt,position) > 1)
        {
            rb.AddForce(-steering.normalized * _vel);
            //_movDir = steering.normalized;
        }
        else if(rb.velocity.magnitude < 5)
            rb.AddForce(-desired.normalized * _vel);

    }

    protected void MoveTo(Vector3 target, float minSpeed)
    {
        if (!isChasing)
        {
            if (DistanceLessThan(0.75f, target))
                movementSpeed = minSpeed;
            else
                movementSpeed = 2;
        }

        Rotate(target);

        rb.MovePosition(transform.position + transform.forward * Time.fixedDeltaTime * movementSpeed);
    }

    private void MoveToWithPathFinding(ref Vector3[] points, ref int index)
    {
        MoveTo(points[index], 2f);
        if (DistanceLessThan(0.35f, points[index]))
        {
            index++;
        }
    }

    private Vector3 GetInvestigationPoint()
    {
       // Calculate a random point in front of the view of the NPC
       // Choose a random direction and then choose a random point in the range
        Vector3 randomDirection = ChooseRandomDirection();

        return transform.position + randomDirection * Random.Range(_investigationMinDistance, _investigationMaxDistance);
    }
    Vector3 ChooseRandomDirection()
    {
        Debug.Log("Chosing direction...");

        float randomAngle = Random.Range(-_viewRange / 2, _viewRange / 2);

        return Quaternion.AngleAxis(randomAngle, Vector3.up) * transform.forward;
    }

    protected bool DistanceLessThan(float distance, Vector3 target)
    {
        return Vector3.Distance(target, transform.position) < distance;
    }

    private void Rotate(Vector3 target)
    {
        //Debug.Log(rb.velocity);
        if (movementSpeed != 0)
        {
            Vector3 goal = new Vector3(target.x, transform.position.y, target.z);

            Vector3 direction = goal - transform.position;
            direction.Normalize();

            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, direction, turnSpeed * Time.fixedDeltaTime, 0f);

            rb.MoveRotation(Quaternion.LookRotation(desiredForward));
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // When reaching a waypoint move to the next or the previous waypoint
        // Also deactivates the used waypoint and activates the next waypoint
        if (other.CompareTag("Waypoint"))
        {
            _waypoints[chosenWaypoint].gameObject.SetActive(false);

            if (chosenWaypoint + 1 == _waypoints.Length)
                movingForward = false;
            else if (chosenWaypoint - 1 < 0)
                movingForward = true;

            if (!movingForward)
                chosenWaypoint--;
            else
                chosenWaypoint++;

            _waypoints[chosenWaypoint].gameObject.SetActive(true);

            float randomNumber = Random.Range(0f, 1f);
            if (randomNumber > 0.7f)
                stopped = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 minimumDirection = Quaternion.AngleAxis(-_viewRange / 2, Vector3.up) * transform.forward;
        Vector3 maximumDirection = Quaternion.AngleAxis(_viewRange / 2, Vector3.up) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + (minimumDirection * _investigationMinDistance), minimumDirection * _investigationMaxDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + (maximumDirection * _investigationMinDistance), maximumDirection * _investigationMaxDistance);
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
                MoveTo(coord,1);
                if(_lastPos == Vector3.up)
                    Rotate(coord);
                if (_dist < 1.8f && is_Contact(_player.position))
                {
                    _followingP = false;
                    break;
                }
            }
        }

        _followingP = false;
    }
    
}
