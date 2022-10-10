using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class NPC : MonoBehaviour
{
    [Header("Patrolling")]
    [SerializeField] private Transform waypointContainer;
    private Transform[] _waypoints;
    private int chosenWaypoint;
    public Observer obs;

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
    [SerializeField] float viewRange = 40;
    [SerializeField] float maximumDistanceCheck = 5;

    [Tooltip("Select the layer you want the NPC to avoid")]
    [SerializeField] LayerMask layerMaskForInvestigation;

    Vector3 minimumDirection;
    Vector3 maximumDirection;

    Vector3 randomDirection;
    Vector3 targetPoint;


    [Header("Chasing")] 
    [SerializeField] private Transform player;
    private Vector3 _movDir;
    [SerializeField] private float _vel = 5.1f;

    private Queue<Vector3> _pendingPositions;
    private Vector3 lastPos;
    
    bool investigationDirectionChosen = false;

    private bool isPatrolling = true;
    private bool isInvestigating = false;
    private bool isChasing = false;

    void Start()
    {
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

        _pendingPositions = new Queue<Vector3>();

        _waypoints[chosenWaypoint].gameObject.SetActive(true);
        obs.onSeePlayer.AddListener(StartChase);
    }

    //Patrol --> Chase
    public void StartChase()
    {
        Debug.Log("Start chase");
        isPatrolling = false;
        isInvestigating = false;
        investigationDirectionChosen = false;
        isChasing = true;
        Vector3 seek = is_Contact();
        if (seek != Vector3.up)
        {
            _pendingPositions.Enqueue(seek);
            lastPos = seek;
            Debug.Log("chasing: " + seek);
        }
        else
        {
            LostPlayer();
        }
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
        //obs.m_MyEvent.AddListener(SeePlayer);
        investigationDirectionChosen = false;
        isInvestigating = false;
        isPatrolling = true;
    }

    private void FixedUpdate()
    {
        if(isPatrolling)
            Patrolling();
        else if(isInvestigating)
            Investigating();
        else if(isChasing)
            Chasing();
        //Debug.Log(rb.velocity);
    }
    

    private void Patrolling()
    {
        if (!stopped)
        {
            MoveTo(_waypoints[chosenWaypoint].position);
        }
        else
        {
            stoppedTimer += Time.deltaTime;
            if (stoppedTimer >= stoppedMaxTime)
                stopped = false;
        }
    }
    void Investigating()
    {
        if(isInvestigating)
        {
            if (!investigationDirectionChosen)    // WHEN INVESTIGATING CHOSE A DIRECTION
            {
                ChooseRandomDirection();
                // Check if this random direction is valid
                if (!Physics.Raycast(transform.position, randomDirection, maximumDistanceCheck, layerMaskForInvestigation))
                {
                    targetPoint = transform.position + randomDirection * maximumDistanceCheck;

                    investigationDirectionChosen = true;
                    //Debug.Log("Direction Chosen!");
                    //Debug.Log(targetPoint);
                }
            }
            else    // WHEN A DIRECTION IS CHOSEN, MOVE TOWARDS THE OBJECTIVE
            {
                MoveTo(targetPoint);
                if (DistanceLessThan(0.8f, targetPoint))
                {
                    
                    EndInvestigation();
                }
            }
        }
    }
    void Chasing()
    {
        Vector3 seek = is_Contact();
        /*if (seek != Vector3.up || _pendingPositions.Count != 0)
        {
            if (seek != Vector3.up && Vector3.Distance(seek,lastPos) < 6)
            {
                Debug.Log(seek);
                _pendingPositions.Enqueue(seek);
                Debug.Log("Enui");
                lastPos = seek;
            }

            if (set_speed(_pendingPositions.Peek(), _pendingPositions.Count == 1))
            {
                _pendingPositions.Dequeue();
                if (_pendingPositions.Count == 0)
                {
                    isChasing = false;
                    Debug.Log("Player chased");
                }
            }
        }*/
        if (seek != Vector3.up || _pendingPositions.Count != 0)
        {
            if (seek != Vector3.up && Vector3.Distance(seek, lastPos) > 5)
            {
                _pendingPositions.Enqueue(seek);
                lastPos = seek;
            }
            MoveTo(_pendingPositions.Peek());
            if (DistanceLessThan(0.9f, player.position))
            {
                isChasing = false;
                Debug.Log("Player is chased");
            }

            if (DistanceLessThan(0.2f, _pendingPositions.Peek()))
            {
                Debug.Log("DeEnui");
                _pendingPositions.Dequeue();
            }
        }
        else
        {
            Debug.Log("I losted the player");
            LostPlayer();
        }
        //Debug.Log(Vector3.Distance(player.position,transform.position));
    }

    Vector3 is_Contact()
    {
        RaycastHit toPlayer;
        if(Physics.Raycast(transform.position, player.position - transform.position, out toPlayer, 100, 7))
        {
            return toPlayer.point;
        }
        return Vector3.up;
    }
    private bool set_speed(Vector3 runnigAt, bool stop)//if stop == true will stop on the point is running, if not, will not stop
    {
        //Changes speed direction to go to the runningPoint

        Vector3 position = transform.position;
        Vector3 desired = runnigAt - position;
        Vector3 steering = desired - rb.velocity;
        Debug.DrawRay(position,steering.normalized * _vel,Color.red);
        steering.Normalize();
        if (Vector3.Angle(desired, steering) < 30 || (stop && (transform.position - runnigAt).magnitude < 10))
        {
            //Debug.Log(steering);
            _movDir = steering;
            rb.AddForce(steering * _vel);
        }
        else
        {
            rb.AddForce(_movDir * _vel);
            //rb.AddForce(desired.normalized * _vel);
        }
        //Debug.Log("D: " + desired);
        Rotate(runnigAt);
        return Math.Abs((transform.position - runnigAt).magnitude) < 0.2f;
    }

    private void MoveTo(Vector3 target)
    {
        if (DistanceLessThan(0.75f, target))
            movementSpeed = isPatrolling ? 1:0;
        else
            movementSpeed = 2;

        Rotate(target);

        rb.MovePosition(transform.position + transform.forward * Time.fixedDeltaTime * movementSpeed);
    }

    private bool DistanceLessThan(float distance, Vector3 target)
    {
        return Vector3.Distance(target, transform.position) < distance;
    }

    private void Rotate(Vector3 target)
    {
        if (rb.velocity != Vector3.zero)
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
    
    void ChooseRandomDirection()
    {
        Debug.Log("Chosing direction...");
        float randomAngle = Random.Range(-viewRange / 2, viewRange / 2);
        randomDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * transform.forward;
    }


    // GIZMO STUFF Visual feedback in editor for easy tweaking of values
    private void OnValidate()
    {
        minimumDirection = Quaternion.AngleAxis(-viewRange / 2, Vector3.up) * transform.forward;
        maximumDirection = Quaternion.AngleAxis(viewRange / 2, Vector3.up) * transform.forward;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, transform.forward * maximumDistanceCheck);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, minimumDirection * maximumDistanceCheck);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, maximumDirection * maximumDistanceCheck);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, randomDirection * maximumDistanceCheck);
    }
    
}
