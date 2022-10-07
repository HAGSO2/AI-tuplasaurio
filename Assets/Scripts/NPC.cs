using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class NPC : MonoBehaviour
{
    [Header("Patrolling")]
    [SerializeField] private Transform waypointContainer;
    private Transform[] _waypoints;
    private int n_waypoint;
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

    private Vector3 _movDir;
    
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


    bool choseDirection = false;

    private bool isPatrolling = true;
    private bool isInvestigating = false;
    private bool isChasing = false;




    void Start()
    {
        stoppedTimer = 0;

        stopped = false;
        movingForward = true;
        rb = GetComponent<Rigidbody>();

        _waypoints = new Transform[waypointContainer.childCount];

        for (int i = 0; i < waypointContainer.childCount; i++)
        {
            _waypoints[i] = waypointContainer.GetChild(i);
        }

        _waypoints[n_waypoint].gameObject.SetActive(true);
        obs.m_MyEvent.AddListener(SeePlayer);
    }

    //Patrol --> Chase
    public void SeePlayer()
    {
        Debug.Log("End Patrolling");
        Debug.Log("Start chase");
        obs.m_MyEvent.RemoveListener(SeePlayer);
        isPatrolling = false;
        isInvestigating = false;
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
    void EndInvestigation()
    {
        Debug.Log("Finished investigating");
        Debug.Log("Start Patrol");
        //obs.m_MyEvent.AddListener(SeePlayer);
        choseDirection = false;
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
    }
    

    

    private void Patrolling()
    {
        if (!stopped)
        {
            MoveTo(_waypoints[n_waypoint].position);
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
            if (!choseDirection)    // WHEN INVESTIGATING CHOSE A DIRECTION
            {
                ChooseRandomDirection();
                // Check if this random direction is valid
                if (!Physics.Raycast(transform.position, randomDirection, maximumDistanceCheck, layerMaskForInvestigation))
                {
                    targetPoint = transform.position + randomDirection * maximumDistanceCheck;

                    choseDirection = true;
                    Debug.Log("Direction Chosen!");
                    Debug.Log(targetPoint);
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
        LostPlayer();
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

    private void OnTriggerEnter(Collider other)
    {
        // When reaching a waypoint move to the next or the previous waypoint
        // Also deactivates the used waypoint and activates the next waypoint
        if (other.CompareTag("Waypoint"))
        {
            _waypoints[n_waypoint].gameObject.SetActive(false);

            if (n_waypoint + 1 == _waypoints.Length)
                movingForward = false;
            else if (n_waypoint - 1 < 0)
                movingForward = true;

            if (!movingForward)
                n_waypoint--;
            else
                n_waypoint++;

            _waypoints[n_waypoint].gameObject.SetActive(true);

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
