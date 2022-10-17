using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class NPC : MonoBehaviour
{
    private Pathfinding _path;
    
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

    private bool chasingPath;

    //private Queue<Vector3> _pendingPositions;
    //private Vector3 lastPos;
    [SerializeField] private GameEnding gameEnding; 
    private 
    
    bool investigationDirectionChosen = false;

    private bool isPatrolling = true;
    private bool isInvestigating = false;
    private bool isChasing = false;

    protected void Start()
    {
        _path = GetComponent<Pathfinding>();
        chasingPath = false;
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
        Debug.Log(seek);
        if (seek == Vector3.up)
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
                MoveTo(targetPoint, 0f);
                if (DistanceLessThan(0.8f, targetPoint))
                {
                    
                    EndInvestigation();
                }
            }
        }
    }
    void Chasing()
    {
//        Debug.Log(seek);
        if (is_Contact() != Vector3.up)
        {
            /*if (DistanceLessThan(0.8f, player.position))
            {
                gameEnding.CaughtPlayer();
            }*/
            if (!chasingPath)
            {
                Debug.Log("I've started calcing the path");
                StartCoroutine(FollowPath(_path.FindPath(transform.position, player.position)));
            }

        }
    }

    Vector3 is_Contact()
    {
        RaycastHit toPlayer;
        if(Physics.Raycast(transform.position, player.position - transform.position, out toPlayer, 100, 7))
        {
            return toPlayer.transform.position;
        }
        return Vector3.up;
    }
    /*
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
    }*/

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

    private void Set_speed(Vector3 running)
    {
        //Debug.Log("Running");
        Vector3 desired = running - transform.position;
        Vector3 steering = desired - rb.velocity;
        steering.Normalize();
        //Debug.Log(steering);
        rb.MovePosition(steering);
    }

    protected bool DistanceLessThan(float distance, Vector3 target)
    {
        return Vector3.Distance(target, transform.position) < distance;
    }

    private void Rotate(Vector3 target)
    {
        Debug.Log(rb.velocity);
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
    private IEnumerator FollowPath(Vector3[] path)
    {
        chasingPath = true;
        Debug.Log("I end up calcing");
        foreach (Vector3 coord in path)
        {
            Debug.Log(coord);
            
            while (DistanceLessThan(0.002f, coord))
            {
                Set_speed(coord);
                Debug.Log(coord);
                yield return new WaitForFixedUpdate();
            }
        }

        //chasingPath = false;
    }
    
}
