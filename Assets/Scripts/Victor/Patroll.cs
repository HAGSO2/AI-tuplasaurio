using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroll : MonoBehaviour
{
    [SerializeField]
    private Transform waypointContainer;
    private Transform[] waypoints;
    private int n_waypoint;

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
    void Start()
    {
        stoppedTimer = 0;

        stopped = false;
        movingForward = true;
        rb = GetComponent<Rigidbody>();

        waypoints = new Transform[waypointContainer.childCount];

        for (int i = 0; i < waypointContainer.childCount; i++)
        {
            waypoints[i] = waypointContainer.GetChild(i);
        }

        waypoints[n_waypoint].gameObject.SetActive(true);
        MoveTo();
    }

    void Update()
    {
        //Debug.Log(n_waypoint);
    }

    private void FixedUpdate()
    {
        Patrolling();
    }

    private void Patrolling()
    {
        if (!stopped)
        {
            MoveTo();

            Rotate(waypoints[n_waypoint]);

        }
        else
        {
            stoppedTimer += Time.deltaTime;
            if (stoppedTimer >= stoppedMaxTime)
                stopped = false;
        }
    }

    private void MoveTo()
    {
        if (DistanceLessThan(0.75f))
            movementSpeed = 1;
        else
            movementSpeed = 2;

        rb.MovePosition(transform.position + transform.forward * Time.fixedDeltaTime * movementSpeed);
    }

    private bool DistanceLessThan(float distance)
    {
        return Vector3.Distance(waypoints[n_waypoint].position, transform.position) < distance;
    }

    private void Rotate(Transform waypoint)
    {
        if (rb.velocity != Vector3.zero)
        {
            Vector3 goal = new Vector3(waypoint.position.x, transform.position.y, waypoint.position.z);

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
            waypoints[n_waypoint].gameObject.SetActive(false);

            if (n_waypoint + 1 == waypoints.Length)
                movingForward = false;
            else if (n_waypoint - 1 < 0)
                movingForward = true;

            if (!movingForward)
                n_waypoint--;
            else
                n_waypoint++;

            waypoints[n_waypoint].gameObject.SetActive(true);

            float randomNumber = Random.Range(0f, 1f);
            if (randomNumber > 0.7f)
                stopped = true;
        }
    }
    /*
    void isPlayerInFront()
    {
        RaycastHit hit;
        if (Physics.Raycast(rayCastTransform.position, transform.forward, out hit, 4f))
        {
            //Debug.Log(hit.transform.tag);
            if (hit.transform.CompareTag("Player"))
            {
                playerIsInFront = true;
            }
            else
            {
                playerIsInFront = false;
            }
        }
        else
        {
            playerIsInFront = false;
        }
    }*/
}
