using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class NPC : MonoBehaviour
{
    public  Pathfinding _pathfinding;
    public Transform reference;
    private List<Transform> references;
    [SerializeField] protected EnemiesManager _manager;
    
    [Header("Patrolling")]
    [SerializeField] private Transform waypointContainer;
    private Transform[] _waypoints;
    private int chosenWaypoint;
    private bool waypointReached = true;
    private List<Node> toWaypoint;

    private Coroutine patrollingPath;

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

    private Coroutine investigationPath;


    [FormerlySerializedAs("player")]
    [Header("Chasing")] 
    [SerializeField] private Transform _player;
    [SerializeField] private float _vel;
    private bool _followingP;
    private Vector3 _lastPos;
    private float _dist;
    //private Queue<Vector3> _pendingPositions;
    //private Vector3 lastPos;
    [SerializeField] private GameEnding gameEnding;  
    
    private bool isPatrolling = true;
    private bool isInvestigating = false;
    private bool isChasing = false;

    private bool endPath = false;   // True when a path finishes. Change manually to false again.

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
        _lastPos = Vector3.up;
    }

    private float isFrozenTimer1 = 0;
    private float isFrozenTimer2 = 0;
    private float isFrozenFirstStop = 1f;
    private float isFrozenSecondStop = 2f;
    private Vector3 previousPosition;

    protected void Update()
    {
        //Check if NPC is frozen
        isFrozenTimer1 += Time.deltaTime;
        isFrozenTimer2 += Time.deltaTime;
        if (isFrozenTimer1 >= isFrozenFirstStop && isFrozenTimer2 <= isFrozenFirstStop)
        {
            isFrozenTimer1 = 0;
            previousPosition = transform.position;
        }
        else if (isFrozenTimer2 >= isFrozenSecondStop)
        {
            isFrozenTimer2 = 0;
            if (DistanceLessThan(0.4f, previousPosition) && !stopped)
            {
                Reset();
            }
        }
    }

    protected void FixedUpdate()
    {
        if (isPatrolling)
            Patrolling();
        else if (isInvestigating)
            Investigating();
        else if (isChasing)
            Chasing();
    }

    private void Reset()
    {
        StopAllCoroutines();
        _followingP = false;
        //endPath = true;
        isInvestigating = false;
        isChasing = false;
        waypointReached = false;
        investigationPointChosen = false;
        isPatrolling = true;
    }

    //Patrol --> Chase
    public void StartChase()
    {
        if (!waypointReached)
        {
            Debug.Log("Stopping patrollingPath");
            StopCoroutine(patrollingPath);
        }

        Debug.Log("Start chase");
        _followingP = false;
        endPath = true;
        isPatrolling = false;
        isInvestigating = false;
        investigationPointChosen = false;
        waypointReached = false;
        isChasing = true;
    }
    
    //Chase --> investigation
    private void LostPlayer()
    {
        Debug.Log("End Chasing");
        Debug.Log("Investigating");
        isChasing = false;
        StopAllCoroutines();
        isInvestigating = true;
    }
    
    //Investigation --> Patrol
    void EndInvestigation() // Only executed when NPC is bored
    {
        StopCoroutine(investigationPath);

        _followingP = false;
        //endPath = true;

        isChasing = false;
        Debug.Log("Finished investigating");
        Debug.Log("Start Patrol");

        waypointReached = false;

        investigationPointChosen = false;
        isInvestigating = false;
        isPatrolling = true;
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
            if (!_followingP && !endPath)
            {
                Debug.Log("Preparing path");
                _pathfinding.FindPath(transform.position, _waypoints[chosenWaypoint].position);
                patrollingPath = StartCoroutine(FollowPath(_pathfinding.finalPath));
            }
            else if (endPath)
            {
                endPath = false;
                waypointReached = true;
            }
            
        }

        if(IsContact(_player.position))  
            StartChase();       
    }

    void Investigating()
    {
        if (!IsContact(_player.position))
        {
            if (!investigationPointChosen)    // WHEN INVESTIGATING CHOOSE A DIRECTION
            {
                walkPoint = GetInvestigationPoint();

                // Check if this random position is valid
                if (Physics.Raycast(walkPoint, -transform.up, 2f, layerMaskForInvestigation))
                {
                    investigationPointChosen = true;
                }
            }
            else // When a point is chosen, walk towards it using the pathfinding
            {          
                if (!_followingP && !endPath) // followingP changes once following. endPath will change once it reached the end
                {
                    _pathfinding.FindPath(transform.position, walkPoint);
                    investigationPath = StartCoroutine(FollowPath(_pathfinding.finalPath));
                    Debug.Log("Moving to investigation target");
                }
                else if(endPath)
                {
                    Debug.Log("Investigation path finished");
                    endPath = false;                 
                    EndInvestigation();
                }
            }
        }
        else
        {
            StopCoroutine(investigationPath);
            Debug.Log("Was investigating, but saw the player");
            waypointReached = true;
            StartChase();
        }
    }
    void Chasing()
    {
        Debug.Log("Chasing");
        Debug.Log("Follow" + _followingP);
        Debug.Log("lastPos" + endPath);
        if (IsContact(_player.position))
        {
            if (!_followingP)
            {
                //Debug.Log("Going to: " + _lastPos);
                _pathfinding.FindPath(transform.position, _lastPos);
                StartCoroutine(FollowPath(_pathfinding.finalPath));
                _lastPos = Vector3.up;
            }
        }

        if (_lastPos == Vector3.up && !_followingP)
        {
            endPath = false;
            LostPlayer();
            return;
        }
        if (!_followingP && _lastPos != Vector3.up )
        {
            //Debug.Log("Going to last: " + _lastPos);
            _pathfinding.FindPath(transform.position, _lastPos);
            StartCoroutine(FollowPath(_pathfinding.finalPath));
            _lastPos = Vector3.up;
        }
    }

    protected bool IsContact(Vector3 looking) //Returns true if the NPC can see the poitn at a max distance
    {
        Vector3 dir = looking - transform.position;
        RaycastHit raycastHit;
        if (Physics.Raycast(transform.position, dir, out raycastHit) && raycastHit.transform.CompareTag("Player"))
        {
            //Debug.Log(raycastHit.transform.name);
            if (Vector3.Angle(transform.forward, dir) < 60)
            {
                _lastPos = looking;
                return true;
            }
        }
        return false;
    }
    /*private void set_speed(Vector3 runnigAt)//if stop == true will stop on the point is running, if not, will not stop
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

        rb.MovePosition(transform.position + (movementSpeed * Time.fixedDeltaTime * transform.forward));
    }

    #region ChoosingInvestigationPoint
    // Calculate a random point in front of the view of the NPC
    // Choose a random direction and then choose a random point in the range
    private Vector3 GetInvestigationPoint()
    {   
        Vector3 randomDirection = ChooseRandomDirection();

        return transform.position + randomDirection * Random.Range(_investigationMinDistance, _investigationMaxDistance);
    }
    Vector3 ChooseRandomDirection()
    {
        Debug.Log("Chosing direction...");

        float randomAngle = Random.Range(-_viewRange / 2, _viewRange / 2);

        return Quaternion.AngleAxis(randomAngle, Vector3.up) * transform.forward;
    }
    #endregion

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

    private IEnumerator FollowPath(List<Node> path)
    {
        _followingP = true;
        endPath = false;
        EntendiLaReferencia(path);
        foreach (Node coord in path)
        {
            while (Vector3.Distance(coord.position, transform.position) > 0.8f)
            {
//                Debug.Log("Moving to " + coord.position);
                MoveTo(coord.position,1);
                yield return new WaitForFixedUpdate();
                //if(!endPath)break;
            }
            //if(!endPath)break;
        }
        Debug.Log("Finished Follow Path");
        _followingP = false;
        endPath = true;
    }

    private void EntendiLaReferencia(List<Node> path)
    {
        references = new List<Transform>(path.Count);
        foreach (Node coord in path)
        {
            references.Add(Instantiate(reference));
            references[^1].position = coord.position;
        }
    }

    public void goToComunicatedLocation(Vector3 position) //Comentar por si se podria usar la funcion chasing o otra que ya haga esto
    {
        //Vector3[] path = 
        //_pathfinding.FindPath(transform.position, position);
        //StartCoroutine(FollowPath(_pathfinding.finalPath));
    }
    
}
